using System.Collections.Concurrent;

namespace StickyNotes.Services
{
    public static class ServiceProvider
    {
        private static readonly ConcurrentDictionary<Type, object> _services = new();
        private static readonly ConcurrentDictionary<Type, Func<object>> _factories = new();
        private static readonly ConcurrentDictionary<Type, Lazy<object>> _lazyServices = new();

        /// <summary>
        /// Registers an existing service instance
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="instance">Existing instance</param>
        /// <returns>True if registration is successful otherwise false</returns>
        /// <exception cref="ArgumentNullException">Throws if the argument is null</exception>
        public static bool RegisterService<T>(T instance) where T : class
        {
            ArgumentNullException.ThrowIfNull(instance);
            var serviceType = typeof(T);

            if (_services.TryRemove(serviceType, out var existingInstance))
            {
                (existingInstance as IDisposable)?.Dispose();
            }
            return _services.TryAdd(serviceType, instance);
        } // RegisterService

        /// <summary>
        /// Registers a factory that will create an instance at the first call
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="factory">Service factory</param>
        /// <exception cref="ArgumentNullException">Throws if the argument is null</exception>
        public static void RegisterService<T>(Func<T> factory) where T : class
        {
            ArgumentNullException.ThrowIfNull(factory);
            var serviceType = typeof(T);
            _factories[serviceType] = factory;
        } // RegisterService

        /// <summary>
        /// Registers a lazy service that will be created at the first call
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="factory">Service factory</param>
        /// <exception cref="ArgumentNullException">Throws if the argument is null</exception>
        public static void RegisterLazyService<T>(Func<T> factory) where T : class
        {
            ArgumentNullException.ThrowIfNull(factory);
            var serviceType = typeof(T);
            _lazyServices[serviceType] = new Lazy<object>(factory);
        } // RegisterLazyService

        /// <summary>
        /// Gets the instance of the previously created service if it exists or null
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>The instance of the given service type or null</returns>
        public static T? GetService<T>() where T : class
        {
            var serviceType = typeof(T);

            // Trying to provide an existing instance
            if (_services.TryGetValue(serviceType, out var instance))
                return (T)instance;

            // Trying to use a factory
            if (_factories.TryGetValue(serviceType, out var factory))
            {
                var newInstance = factory();
                if (newInstance != null)
                {
                    _services.TryAdd(serviceType, newInstance);
                    return (T)newInstance;
                }
            }

            // Trying a lazy service
            if (_lazyServices.TryGetValue(serviceType, out var lazy))
            {
                var lazyInstance = lazy.Value;
                _services.TryAdd(serviceType, lazyInstance);
                return (T)lazyInstance;
            }
            return null;
        } // GetService

        /// <summary>
        /// Gets the instance of the previously created service if it exists or will throw an exception if it doesn't
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>The instance of the given service type or null</returns>
        /// <exception cref="InvalidOperationException">Throws if no such service exists</exception>
        public static T GetRequiredService<T>() where T : class
        {
            var service = GetService<T>();
            return service ?? throw new InvalidOperationException($"Service of type {typeof(T)} is not registered.");
        } // GetRequiredService

        /// <summary>
        /// Checks if the service of a given type is registered
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>True if service exists otherwise false</returns>
        public static bool IsServiceRegistered<T>() where T : class
        {
            var serviceType = typeof(T);
            return _services.ContainsKey(serviceType) ||
                   _factories.ContainsKey(serviceType) ||
                   _lazyServices.ContainsKey(serviceType);
        } // IsServiceRegistered

        /// <summary>
        /// Removes the unnecessary service type
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>True if successful otherwise false</returns>
        public static bool UnregisterService<T>() where T : class
        {
            var serviceType = typeof(T);
            var removed = false;

            if (_services.TryRemove(serviceType, out var instance))
            {
                (instance as IDisposable)?.Dispose();
                removed = true;
            }

            _factories.TryRemove(serviceType, out _);
            _lazyServices.TryRemove(serviceType, out _);
            return removed;
        } // UnregisterService

        /// <summary>
        /// Clears all services
        /// </summary>
        public static void Clear()
        {
            foreach (var instance in _services.Values)
            {
                (instance as IDisposable)?.Dispose();
            }

            _services.Clear();
            _factories.Clear();
            _lazyServices.Clear();
        } // Clear
    } // class ServiceProvider
} // namespace