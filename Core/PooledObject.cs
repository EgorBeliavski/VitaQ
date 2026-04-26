using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitaQ
{
    public sealed class PooledObject<T> : IDisposable  where T : class 
    {
        private T? _item;
        private bool _isReturn;
        private readonly Action<T> _returnAction; 
        public PooledObject(T? item, Action<T> returnAction)
        {
            _item = item;
            _returnAction = returnAction;
        }

        public T Value{
            get{
                if(_isReturn){
                    throw new ObjectDisposedException(nameof(PooledObject<T>), "Object already was return in pool");
                }
                return _item;
            }
        }
        public void Dispose(){
            if(!_isReturn){
                if( _item != null ){
                    
                    _returnAction(_item);
                    _item = null;
                }
                _isReturn = true;
            }
        }

       
    }
}
