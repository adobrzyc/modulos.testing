using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Modulos.Testing;

// ReSharper disable UnusedMember.Global

namespace SimpleDomain.Tests
{
    public static class AutofacExtensions
    {
        public static void UpdateIoc(this TestOptions options, Action<IServiceProvider, ContainerBuilder> update)
        {
            if (update == null) throw new ArgumentNullException(nameof(update));
            
            options.CreateScope = sp =>
            {
                var scope = ((AutofacServiceProvider) sp).LifetimeScope.BeginLifetimeScope(builder =>
                {
                    update(sp, builder);
                });
                
                return new AutofacServiceScope(scope);
            };
        }

        public static void UpdateIoc(this TestOptions options, Action<ContainerBuilder> update)
        {
            if (update == null) throw new ArgumentNullException(nameof(update));
            
            options.CreateScope = sp =>
            {
                var scope = ((AutofacServiceProvider) sp).LifetimeScope
                    .BeginLifetimeScope(update);
                
                return new AutofacServiceScope(scope);
            };
        }

        private sealed class AutofacServiceScope : IServiceScope, IAsyncDisposable
        {
            private bool disposed;
            private readonly AutofacServiceProvider serviceProvider;

            /// <summary>
            /// Initializes a new instance of the <see cref="Autofac.Extensions.DependencyInjection.AutofacServiceScope"/> class.
            /// </summary>
            /// <param name="lifetimeScope">
            /// The lifetime scope from which services should be resolved for this service scope.
            /// </param>
            public AutofacServiceScope(ILifetimeScope lifetimeScope)
            {
                serviceProvider = new AutofacServiceProvider(lifetimeScope);
            }

            /// <summary>
            /// Gets an <see cref="IServiceProvider" /> corresponding to this service scope.
            /// </summary>
            /// <value>
            /// An <see cref="IServiceProvider" /> that can be used to resolve dependencies from the scope.
            /// </value>
            public IServiceProvider ServiceProvider => serviceProvider;

            /// <summary>
            /// Disposes of the lifetime scope and resolved disposable services.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources.
            /// </summary>
            /// <param name="disposing">
            /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.
            /// </param>
            private void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    disposed = true;

                    if (disposing)
                    {
                        serviceProvider.Dispose();
                    }
                }
            }

            public async ValueTask DisposeAsync()
            {
                if (!disposed)
                {
                    disposed = true;
                    await serviceProvider.DisposeAsync();
                }
            }
        }
    }
}