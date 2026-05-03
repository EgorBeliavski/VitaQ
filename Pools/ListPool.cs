
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Text;




namespace VitaQ
{
    public class ListPool<T> : IAdvanced<List<T>>, ISafePool<List<T>>
    {
        private const int MaxCount = 100000;

        private static readonly Meter _meter = new("VitaQ", "1.1.0");

        private readonly  ConcurrentQueue<List<T>> _pool = new();

        private readonly int _maxSize = 100;

        private static int _active;
        private static int _hits;
        private static int _misses;

        static ListPool()
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
        public PooledObject<List<T>> Get()
        {
            return new PooledObject<List<T>>(Borrow(), Return);
        }
        public List<T> Borrow()
        {
            if (_pool.TryDequeue(out var item))
            {

                Interlocked.Increment(ref _hits);
                Interlocked.Increment(ref _active);
                return item;


            }

            Interlocked.Increment(ref _misses);
            Interlocked.Increment(ref _active);
            return new List<T>();
        }
        public void Return(List<T> item)
        {
            if (item == null) return;

            Interlocked.Decrement(ref _active);


            

            item.Clear();

            if (_pool.Count < _maxSize)
            {
                _pool.Enqueue(item);
            }
        }

        

        public void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var newItem = new List<T>();
                Return(newItem);
            }
        }

        public void Clear()
        {
            while (_pool.TryDequeue(out _)) { }
            Interlocked.Exchange(ref _active, 0);
            Interlocked.Exchange(ref _hits, 0);
            Interlocked.Exchange(ref _misses, 0);
        }

       

    }
}
