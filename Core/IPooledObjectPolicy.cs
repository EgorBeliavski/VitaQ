using System;
using System.Collections.Generic;
using System.Text;

namespace VitaQ
{
    public interface IPooledObjectPolicy<T> where T : class
    {
        

        T Borrow();

        void Return(T obj);

        void PreWarm(int count);

        void Clear();
    }
}

