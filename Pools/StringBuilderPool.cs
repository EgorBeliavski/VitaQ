using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using VitaQ;



namespace VitaQ
{
    


    public class StringBuilderPool : IAdvanced<StringBuilder>,ISafePool<StringBuilder>
    {


        private  readonly Meter _meter = new("VitaQ", "1.1.0");

        private const int MaxCapacityChars = 5_000_000;
        private const int DefaultCapacity = 256;


        private readonly  ConcurrentQueue<StringBuilder> _pool = new();

        
        private readonly  ThreadLocal<Queue<StringBuilder>> _localpool = new(() => new Queue<StringBuilder>(),trackAllValues:true);


        private readonly int _maxSize = 100;

        private readonly int _localmaxSize = 5;
        

        private  int _active;
        private  int _hits;
        private  int _misses;

        public StringBuilderPool()
        {
            
            _meter.CreateObservableGauge("app.state.active", () => Volatile.Read(ref _active));
            _meter.CreateObservableGauge("app.state.hits", () => Volatile.Read(ref _hits));
            _meter.CreateObservableGauge("app.state.misses", () => Volatile.Read(ref _misses));
        }
        [Obsolete("Use OpenTelemetry metrics 'app.state.active' instead.")]
        public int ActiveCount => Volatile.Read(ref _active);

        [Obsolete("Use OpenTelemetry metrics 'app.state.hits' instead.")]
        public int Hits => Volatile.Read(ref _hits);

        [Obsolete("Use OpenTelemetry metrics 'app.state.misses' instead.")]
        public int Misses => Volatile.Read(ref _misses);

        
        public PooledObject<StringBuilder> Get()
        {    
            return new PooledObject<StringBuilder>(Borrow(), Return);
        }

        
        public StringBuilder Borrow()
        {
            
            if (_localpool.Value.Count>0)
            {

                Interlocked.Increment(ref _hits);
                Interlocked.Increment(ref _active);

                return _localpool.Value.Dequeue();


            }
             
            if (_pool.TryDequeue(out var item))
            {

                Interlocked.Increment(ref _hits);
                Interlocked.Increment(ref _active);

                return item;
            }

            Interlocked.Increment(ref _misses);
            Interlocked.Increment(ref _active);
            return new StringBuilder(DefaultCapacity);
        }

        public void Return(StringBuilder item)
        {
            if (item == null) return;
                Interlocked.Decrement(ref _active);

            
            if (item.Capacity > MaxCapacityChars)
            {
                return; 
            }

            item.Length = 0;
            if (_localpool.Value.Count < _localmaxSize)
            {
                _localpool.Value.Enqueue(item);
                return;
            }
            if (_pool.Count < _maxSize)
            {
                _pool.Enqueue(item);
            }
        }


        public void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var newItem = new StringBuilder();
                Return(newItem);
            }
        }


        public void Clear()
        {
            foreach(var item in _localpool.Values){
               item.Clear();
            }
            while (_pool.TryDequeue(out _)) { }
            Interlocked.Exchange(ref _active, 0);
            Interlocked.Exchange(ref _hits, 0);
            Interlocked.Exchange(ref _misses, 0);
        }

       


    }
}
