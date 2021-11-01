namespace Modulos.Testing
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Base definition of the test created via <see cref="ITestEnvironment" />.
    /// </summary>
    public interface ITest : IServiceScope, IServiceProvider, IAsyncDisposable
    {
        /// <summary>
        /// Executes <see cref="ITestWrapper.Begin" /> method of associated wrappers.
        /// </summary>
        Task BeginWrappers();

        /// <summary>
        /// Executes <see cref="ITestWrapper.Finish" /> method of associated wrappers.
        /// </summary>
        Task FinishWrappers();
    }
}