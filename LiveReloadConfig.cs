using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace TheElm.Services {
    /// <summary>
    /// Config that can be refreshed when the backing file is changed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class LiveReloadConfig<T> : IOptions<T>, IObservable<T>, IDisposable where T : class {
        private readonly string SectionName;
        private readonly ILogger<LiveReloadConfig<T>> Logger;
        private readonly IDisposable Watcher;
        private readonly List<Subscriber> Subscribers = [];
        
        /// <inheritdoc />
        T IOptions<T>.Value => this.Value ?? throw new NullReferenceException($"Value of \"{this.SectionName}\" for \"{typeof(T).FullName}\" not currently set");
        private T? Value = null;
        
        public LiveReloadConfig(
            string name,
            ILogger<LiveReloadConfig<T>> logger,
            IConfiguration config,
            T? defaultVal
        ) {
            this.SectionName = name;
            this.Logger = logger;
            this.Value = config.Get<T>() ?? defaultVal;
            
            // Watch for any changes and update our config live
            this.Watcher = ChangeToken.OnChange(
                () => config.GetReloadToken(),
                () => {
                    if (config.Get<T>() is T value) {
                        logger.LogInformation("Updated configuration value");
                        this.Value = value;
                        
                        foreach ( LiveReloadConfig<T>.Subscriber subscriber in this.Subscribers ) {
                            subscriber.Observer.OnNext(value);
                        }
                    } else {
                        logger.LogError("Failed to parse config");
                    }
                }
            );
        }
        
        /// <inheritdoc />
        public IDisposable Subscribe( IObserver<T> observer ) {
            Subscriber subscriber = new(this, observer);
            
            this.Subscribers.Add(subscriber);
            if ( this.Value is not null ) {
                observer.OnNext(this.Value);
            }
            
            this.Logger.LogInformation("Added configuration observer");
            return subscriber;
        }
        
        /// <inheritdoc />
        public void Dispose() {
            foreach ( LiveReloadConfig<T>.Subscriber subscriber in this.Subscribers ) {
                subscriber.Observer.OnCompleted();
            }
            
            this.Subscribers.Clear();
            this.Watcher.Dispose();
        }
        
        private sealed class Subscriber : IDisposable {
            private readonly LiveReloadConfig<T> Parent;
            public readonly IObserver<T> Observer;
            
            public Subscriber( LiveReloadConfig<T> parent, IObserver<T> observer ) {
                this.Parent = parent;
                this.Observer = observer;
            }
            
            /// <inheritdoc />
            public void Dispose() {
                this.Parent.Logger.LogInformation("Removed configuration observer");
                this.Parent.Subscribers.Remove(this);
            }
        }
    }
}
