using System;
using System.Collections.Generic;
using System.Text;

namespace VitaQ
{
    public interface IPooledObjectPolicy<T> where T : class
    {
        T Create();

       
        void Return(T obj);

       
        bool CanReturn(T obj);
    }
}
