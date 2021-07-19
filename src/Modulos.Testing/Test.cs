using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Modulos.Testing
{
    public class Test : ITest
    {
        private readonly TestOptions _options;
        private readonly IServiceScope _scope;
        private readonly ConcurrentQueue<ITestWrapper> _wrappers = new ConcurrentQueue<ITestWrapper>();

        public Test(IServiceProvider serviceProvider, TestOptions options)
        {
            _options = options;

            _scope = options.CreateScope != null 
                ? options.CreateScope(serviceProvider) 
                : serviceProvider.CreateScope();
        }

        public IServiceProvider ServiceProvider => _scope.ServiceProvider;

        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        async Task ITest.BeginWrappers()
        {
            foreach (var wrapperType in _options.GetWrappers())
            {
                var wrapper = ResolveWrapper(wrapperType);
                _wrappers.Enqueue(wrapper);
                await wrapper.Begin();
            }
        }

        async Task ITest.FinishWrappers()
        {
            var exceptions = new List<Exception>();
          
            while (_wrappers.Count > 0)
            {
                _wrappers.TryDequeue(out var wrapper);
                try
                {
                    await wrapper.Finish();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
            
            if (exceptions.Any())
            {
                throw new AggregateException("Error(s) during finishing test wrappers." ,exceptions);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scope?.Dispose();
                ((ITest)this).FinishWrappers().Wait();
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            await ((ITest)this).FinishWrappers().ConfigureAwait(false);

            if (_scope is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                _scope.Dispose();
            }
        }
      
        private ITestWrapper ResolveWrapper(Type typeToResolve)
        {
            var ctor = typeToResolve.GetConstructors().Select(e => (ctor: e, count: e.GetParameters().Length))
                .OrderByDescending(e => e.count)
                .Select(e => e.ctor)
                .First();

            var parameters = new List<object>();
            foreach (var param in ctor.GetParameters())
            {
                if (typeof(ITest).IsAssignableFrom(param.ParameterType))
                {
                    parameters.Add(this);
                    continue;
                }
                
                var value = _scope.ServiceProvider.GetRequiredService(param.ParameterType);

                parameters.Add(value);
            }

            return (ITestWrapper)Activator.CreateInstance(typeToResolve, parameters.ToArray());
        }
    }
}