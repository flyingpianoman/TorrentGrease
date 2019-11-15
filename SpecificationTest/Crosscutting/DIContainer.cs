using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SpecificationTest.Crosscutting
{
    public sealed class DIContainer : IAsyncDisposable
    {
        private readonly Dictionary<Type, object> _services;

        public DIContainer()
        {
            _services = new Dictionary<Type, object>();
        }

        public void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        public T Get<T>()
        {
            return (T)_services[typeof(T)];
        }

        public async ValueTask DisposeAsync()
        {
            var services = _services.Values.Where(s => s != null).ToArray();

            foreach (var asyncDisp in services.Where(s => typeof(IAsyncDisposable).IsAssignableFrom(s.GetType())))
            {
                await ((IAsyncDisposable)asyncDisp).DisposeAsync();
            }

            foreach (var disposable in services.Where(s => typeof(IDisposable).IsAssignableFrom(s.GetType())))
            {
                ((IDisposable)disposable).Dispose();
            }
        }

        public static DIContainer Default { get; private set; } = new DIContainer();
    }
}
