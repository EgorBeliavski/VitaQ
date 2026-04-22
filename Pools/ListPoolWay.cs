
using System.Collections.Concurrent;



namespace VitaQ
{
    public static class ListPoolWay<T>
    {
        private const int MaxCapacity = 100000;
        private const int DefaultCapacity = 20;


        private readonly static ConcurrentQueue<List<T>> _pool = new();

        private readonly static int _maxSize = 100;

        private static int _active;
        private static int _hits;
        private static int _misses;

       
        public static List<T> Borrow()
        {
            if (_pool.TryDequeue(out var item))
            {

                Interlocked.Increment(ref _hits);
                Interlocked.Increment(ref _active);
                return item;


            }

            Interlocked.Increment(ref _misses);
            Interlocked.Increment(ref _active);
            return new List<T>(DefaultCapacity);
        }
        public static void Return(List<T> item)
        {
            if (item == null) return;

            Interlocked.Decrement(ref _active);


            if (item.Capacity > MaxCapacity)
            {
                return;
            }

            item.Clear();

            if (_pool.Count < _maxSize)
            {
                _pool.Enqueue(item);
            }
        }

        public static void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var newItem = new List<T>(DefaultCapacity);
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
