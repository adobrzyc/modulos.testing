// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Modulos.Testing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;

    public class Test : ITest
    {
        private readonly ITestOptions options;
        private readonly IServiceScope scope;
        private readonly ConcurrentQueue<ITestWrapper> wrappers = new();

        public Test(IServiceProvider serviceProvider, ITestOptions options)
        {
            this.options = options;

            scope = serviceProvider.CreateScope();
        }

        public IServiceProvider ServiceProvider => scope.ServiceProvider;

        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        async Task ITest.BeginWrappers()
        {
            foreach (var wrapperType in options.GetWrappers())
            {
                var wrapper = ResolveWrapper(wrapperType);
                wrappers.Enqueue(wrapper);
                await wrapper.Begin();
            }
        }

        async Task ITest.FinishWrappers()
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

            if (exceptions.Any()) throw new AggregateException("Error(s) during finishing test wrappers.", exceptions);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                scope?.Dispose();
                ((ITest)this).FinishWrappers().Wait();
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            await ((ITest)this).FinishWrappers().ConfigureAwait(false);

            if (scope is IAsyncDisposable disposable)
                await disposable.DisposeAsync().ConfigureAwait(false);
            else
                scope.Dispose();
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