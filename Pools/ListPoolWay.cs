using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VitaQ
{
    public class ListPoolWay<T> : IPooledObjectPolicy<List<T>>
    {
        private const int MaxCapacity = 100000;
        private const int DefaultCapacity = 20;


        private readonly ConcurrentQueue<List<T>> _pool = new();

        private readonly int _maxSize;

        private int _active;
        private int _hits;
        private int _misses;

        public ListPoolWay(int maxSize = 100)
        {

            _maxSize = maxSize;
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
            return new List<T>(DefaultCapacity);
        }
        public void Return(List<T> item)
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

        public void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var newItem = new List<T>(DefaultCapacity);
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
