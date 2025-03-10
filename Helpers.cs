using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TheElm.Services {
    public static partial class Helpers {
        public static T GetRequiredOptions<T>( this IServiceProvider provider ) where T : class {
            _ = provider ?? throw new ArgumentNullException(nameof(provider));
            
            return provider.GetRequiredService<IOptions<T>>()
                .Value;
        }
        
        public static ILogger<T> GetRequiredLogger<T>( this IServiceProvider provider ) {
            _ = provider ?? throw new ArgumentNullException(nameof(provider));
            
            return provider.GetRequiredService<ILogger<T>>();
        }
        
        public static bool TryGetService<TS>( this IServiceProvider provider, [NotNullWhen(true)] out TS? service ) where TS : class {
            _ = provider ?? throw new ArgumentNullException(nameof(provider));
            
            object? fetch = provider.GetService(typeof(TS));
            
            if (fetch is TS cast) {
                service = cast;
                return true;
            }
            
            service = null;
            return false;
        }
    }
}
