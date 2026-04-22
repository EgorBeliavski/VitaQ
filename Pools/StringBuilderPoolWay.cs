using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using VitaQ;


namespace VitaQ
{
    

    public class StringBuilderPoolWay : IPooledObjectPolicy<StringBuilder>
    {
        private const int MaxCapacityChars = 5_000_000;
        private const int DefaultCapacity = 256;


        private readonly ConcurrentQueue<StringBuilder> _pool = new();

        private readonly int _maxSize;

        private int _active;
        private int _hits;
        private int _misses;

        public StringBuilderPoolWay(int maxSize = 100)
        {

            _maxSize = maxSize;
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
            return new StringBuilder();
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

        public int ReturnActive() => Interlocked.CompareExchange(ref _active, 0, 0);
        public int ReturnHits() => Interlocked.CompareExchange(ref _hits, 0, 0);
        public int ReturnMisses() => Interlocked.CompareExchange(ref _misses, 0, 0);


    }
}
