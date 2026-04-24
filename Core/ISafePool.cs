using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitaQ.Core
{
    public interface ISafePool<T> where T : class
    {
        PooledObject<T> Get();
    }
}
