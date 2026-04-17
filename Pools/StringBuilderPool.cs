using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using VitaQ;


namespace VitaQ
{
    

    public class StringBuilderPool : IPooledObjectPolicy<StringBuilder>
    {
        private const int MaxCapacityChars = 5_000_000;
        private const int DefaultCapacity = 256;

       
        public StringBuilder Create()
        {
            return new StringBuilder(DefaultCapacity);
        }

        
        public void Return(StringBuilder item)
        {
            
            if (item != null)
            {
                item.Length = 0;
            }
        }

        
        public bool CanReturn(StringBuilder item)
        {
            if (item == null) return false;
            return item.Capacity <= MaxCapacityChars;
        }

       
    }
}
