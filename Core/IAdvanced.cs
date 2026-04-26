using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitaQ
{
    public interface IAdvanced<T> where T : class
    {

        [EditorBrowsable(EditorBrowsableState.Never)]
        T Borrow();


        [EditorBrowsable(EditorBrowsableState.Never)]
        void Return(T item);


        [EditorBrowsable(EditorBrowsableState.Never)]
        void PreWarm(int value);


        [EditorBrowsable(EditorBrowsableState.Never)]
        void Clear();

        
    }
}
