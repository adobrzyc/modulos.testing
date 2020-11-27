using System.Threading.Tasks;
using Examples.From.Documentation.Basics.Blocks;
using Modulos.Testing;
using Xunit;

// ReSharper disable MemberCanBeProtected.Global

namespace Examples.From.Documentation.Basics.Environments
{
    public class DefaultEnvironment : TestEnvironment, IAsyncLifetime
    {
        public DefaultEnvironment()
        {
            Add<InitializeIoc>();
        }

        async Task IAsyncLifetime.InitializeAsync()
        {
            await Build();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await DisposeAsync();
        }
    }
}