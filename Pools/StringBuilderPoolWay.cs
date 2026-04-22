using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using VitaQ;


namespace VitaQ
{
    

    public static class StringBuilderPoolWay
    {
        private const int MaxCapacityChars = 5_000_000;
        private const int DefaultCapacity = 256;


        private static readonly ConcurrentQueue<StringBuilder> _pool = new();

        private readonly static int _maxSize = 100;

        private static int _active;
        private static int _hits;
        private static int _misses;

        

        public static StringBuilder Borrow()
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
        public static void Return(StringBuilder item)
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

        public static void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var newItem = new StringBuilder();
                Return(newItem);
            }
        }

        public static void Clear()
        {
            while (_pool.TryDequeue(out _)) { }
            Interlocked.Exchange(ref _active, 0);
            Interlocked.Exchange(ref _hits, 0);
            Interlocked.Exchange(ref _misses, 0);
        }

        public static int ReturnActive() => Interlocked.CompareExchange(ref _active, 0, 0);
        public static int ReturnHits() => Interlocked.CompareExchange(ref _hits, 0, 0);
        public static int ReturnMisses() => Interlocked.CompareExchange(ref _misses, 0, 0);


    }
}
