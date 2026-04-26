
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VitaQ;

namespace VitaQ
{

    public static class ServiceCollectionExtensions
    {
        
        
        public static IServiceCollection AddStringBuilderPool(this IServiceCollection services)
        {
            var pool =  new StringBuilderPool();
            
            return services.AddSingleton<ISafePool<StringBuilder>>(pool);
        }

        public static IServiceCollection AddListPool<T>(this IServiceCollection services) 
        {
            var pool = new ListPool<T>();

            return services.AddSingleton<ISafePool<List<T>>>(pool);
        }
    }
}
