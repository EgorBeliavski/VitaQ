using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using VitaQ;



namespace VitaQ
{
    


    public class StringBuilderPool : IAdvanced<StringBuilder>,ISafePool<StringBuilder>
    {
       



        private const int MaxCapacityChars = 5_000_000;
        private const int DefaultCapacity = 256;


        private readonly static ConcurrentQueue<StringBuilder> _pool = new();

        private readonly int _maxSize = 100;

        private static int _active;
        private static int _hits;
        private static int _misses;

     
        public int ActiveCount => Volatile.Read(ref _active);
        public int Hits => Volatile.Read(ref _hits);
        public int Misses => Volatile.Read(ref _misses);


        public PooledObject<StringBuilder> Get()
        {    
            return new PooledObject<StringBuilder>(Borrow(), Return);
        }

        
        public StringBuilder Borrow()
        {

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
            while (_pool.TryDequeue(out _)) { }
            Interlocked.Exchange(ref _active, 0);
            Interlocked.Exchange(ref _hits, 0);
            Interlocked.Exchange(ref _misses, 0);
        }

       


    }
}
