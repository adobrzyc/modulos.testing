// ReSharper disable MemberCanBeProtected.Global

namespace Examples.From.Documentation.Basics.Environments
{
    using System.Threading.Tasks;
    using Blocks;
    using Modulos.Testing;
    using Xunit;

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