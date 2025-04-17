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
        
        #region Service Injections
        
        /// <summary>
        /// Registers an action used to configure a particular type of options. Note: These are run before all
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> to add the services to</param>
        /// <param name="configureOptions">The action used to configure the options</param>
        /// <typeparam name="TOptions">The options type to be configured</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained</returns>
        public static IServiceCollection Configure<TOptions>( this IServiceCollection collection, Action<TOptions, IServiceProvider> configureOptions ) where TOptions : class
            => collection.Configure(Options.DefaultName, configureOptions);
        
        /// <summary>
        /// Registers an action used to configure a particular type of options. Note: These are run before all
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> to add the services to</param>
        /// <param name="name">The name of the options instances</param>
        /// <param name="configureOptions">The action used to configure the options</param>
        /// <typeparam name="TOptions">The options type to be configured</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained</returns>
        public static IServiceCollection Configure<TOptions>( this IServiceCollection collection, string? name, Action<TOptions, IServiceProvider> configureOptions ) where TOptions : class
            => collection.AddOptions()
                .AddSingleton<IConfigureOptions<TOptions>>(services => new ConfigureNamedOptions<TOptions>(name, options => configureOptions(options, services)));
        
        /// <summary>
        /// Registers an action used to configure all instances of a particulate type of options
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> to add the services to</param>
        /// <param name="configureOptions">The action used to configure the options</param>
        /// <typeparam name="TOptions">The options type to be configured</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained</returns>
        public static IServiceCollection ConfigureAll<TOptions>( this IServiceCollection collection, Action<TOptions, IServiceProvider> configureOptions ) where TOptions : class
            => collection.Configure(name: null, configureOptions: configureOptions);
        
        /// <summary>
        /// Registers an action used to initialize a particular type of options. Note: These are run after all.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> to add the services to</param>
        /// <param name="configureOptions">The action used to configure the options</param>
        /// <typeparam name="TOptions">The options type to be configured</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained</returns>
        public static IServiceCollection PostConfigure<TOptions>( this IServiceCollection collection, Action<TOptions, IServiceProvider> configureOptions ) where TOptions : class
            => collection.PostConfigure(Options.DefaultName, configureOptions);
        
        /// <summary>
        /// Registers an action used to initialize a particular type of options. Note: These are run after all.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> to add the services to</param>
        /// <param name="name"></param>
        /// <param name="configureOptions"></param>
        /// <typeparam name="TOptions">The options type to be configured</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained</returns>
        public static IServiceCollection PostConfigure<TOptions>( this IServiceCollection collection, string? name, Action<TOptions, IServiceProvider> configureOptions ) where TOptions : class
            => collection.AddOptions()
                .AddSingleton<IPostConfigureOptions<TOptions>>(services => new PostConfigureOptions<TOptions>(name, options => configureOptions(options, services)));
        
        /// <summary>
        /// Registers an action used to post configure all instances of a particular type of options. Note: These are run after all.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> to add the services to</param>
        /// <param name="configureOptions"></param>
        /// <typeparam name="TOptions">The options type to be configured</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained</returns>
        public static IServiceCollection PostConfigureAll<TOptions>( this IServiceCollection collection, Action<TOptions, IServiceProvider> configureOptions ) where TOptions : class
            => collection.PostConfigure(name: null, configureOptions: configureOptions);
        
        #endregion
    }
}
