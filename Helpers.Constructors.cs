using Microsoft.Extensions.DependencyInjection;

namespace TheElm.Services {
    public static partial class Helpers {
        public static T Create<T>( this IServiceProvider services, params object[] parameters )
            => ActivatorUtilities.CreateInstance<T>(services, parameters);
        
        public static object Create( this IServiceProvider services, Type type, params object[] parameters )
            => ActivatorUtilities.CreateInstance(services, type, parameters);
    }
}
