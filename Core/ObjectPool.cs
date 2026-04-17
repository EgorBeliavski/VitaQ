using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using VitaQ;

namespace VitaQ
{
    public class ObjectPool<T> where T : class
    {
        private readonly ConcurrentQueue<T> _pool = new();
        private readonly IPooledObjectPolicy<T> _policy;
        private readonly int _maxSize;

        private int _active;
        private int _hits;
        private int _misses;

        public ObjectPool(IPooledObjectPolicy<T> policy, int maxSize = 100)
        {
            _policy = policy;
            _maxSize = maxSize;
        }
      
        public T Borrow()
        {
            if (_pool.TryDequeue(out var item))
            {
                if (_policy.CanReturn(item))
                {
                    Interlocked.Increment(ref _hits);
                    Interlocked.Increment(ref _active);
                    return item;
                }
                
            }

            Interlocked.Increment(ref _misses);
            Interlocked.Increment(ref _active);
            return _policy.Create();
        }
        public void Return(T item)
        {
            if (item == null) return;
                Interlocked.Decrement(ref _active);
            if (!_policy.CanReturn(item))
            {
                return;
            }
            if (_pool.Count < _maxSize)
            {
                _policy.Return(item);
                _pool.Enqueue(item);

            }
        }
        public void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var newItem = _policy.Create();

                if (!_policy.CanReturn(newItem))
                {
                    continue; 
                }

               
                if (_pool.Count < _maxSize)
                {
                    _policy.Return(newItem);
                    _pool.Enqueue(newItem);
                }
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

