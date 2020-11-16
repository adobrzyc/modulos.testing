using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Modulos.Testing
{
    public interface ITest : IServiceScope, IServiceProvider, IAsyncDisposable
    {
        Task BeginWrappers();

        Task FinishWrappers();
    }
}