using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Modulos.Testing
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Test : ITest
    {
        private readonly TestOptions options;
        private readonly IServiceScope scope;
        private readonly ConcurrentQueue<ITestWrapper> wrappers = new ConcurrentQueue<ITestWrapper>();

        public Test(IServiceProvider serviceProvider, TestOptions options)
        {
            this.options = options;

            scope = options.CreateScope != null 
                ? options.CreateScope(serviceProvider) 
                : serviceProvider.CreateScope();
        }

        public IServiceProvider ServiceProvider => scope.ServiceProvider;

        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        public async Task BeginWrappers()
        {
            foreach (var wrapperType in options.GetWrappers())
            {
                var wrapper = ResolveWrapper(wrapperType);
                wrappers.Enqueue(wrapper);
                await wrapper.Begin();
            }
        }

        public async Task FinishWrappers()
        {
            var exceptions = new List<Exception>();
          
            while (wrappers.Count > 0)
            {
                wrappers.TryDequeue(out var wrapper);
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
                scope?.Dispose();
                FinishWrappers().Wait();
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            await FinishWrappers().ConfigureAwait(false);

            if (scope is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                scope.Dispose();
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
                
                var value = scope.ServiceProvider.GetRequiredService(param.ParameterType);

                parameters.Add(value);
            }

            return (ITestWrapper)Activator.CreateInstance(typeToResolve, parameters.ToArray());
        }

    }
}