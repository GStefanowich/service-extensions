using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TheElm.Services {
    public static partial class Helpers {
        /// <summary>
        /// Registers a Configuration object where properties are bound with <see cref="T:Microsoft.Extensions.Configuration.ConfigurationKeyNameAttribute"/>
        ///
        /// Where permitted from ConfigurationSources that provide a <see cref="T:Microsoft.Extensions.Primitives.IChangeToken"/>, updated objects will be constructed when changes are made.
        /// Changes may be retrieved using <see cref="IOptions{T}"/> or observed <see cref="IObservable{T}"/>
        /// </summary>
        /// <param name="collection">Service collection to register on</param>
        /// <param name="key">The root configuration key for the type</param>
        /// <param name="defaultVal"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddLiveConfig<T>( this IServiceCollection collection, string key, T? defaultVal = null ) where T : class {
            // Add the wrapper
            collection.AddSingleton<LiveReloadConfig<T>>(services => {
                IConfiguration configuration = services.GetRequiredService<IConfiguration>();
                return new LiveReloadConfig<T>(key, services.GetRequiredLogger<LiveReloadConfig<T>>(), configuration.GetSection(key), defaultVal);
            });
            
            // Add an accessor
            collection.AddSingleton<IOptions<T>>(services => services.GetRequiredService<LiveReloadConfig<T>>());
            
            // Add a way to listen for when config changes occur
            collection.AddSingleton<IObservable<T>>(services => services.GetRequiredService<LiveReloadConfig<T>>());
            
            // Add a way to directly get the current copy of the class
            collection.AddTransient<T>(services => services.GetRequiredOptions<T>());
        }
    }
}
