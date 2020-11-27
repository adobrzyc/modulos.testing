using Examples.From.Documentation.Basics.Blocks;
using Examples.From.Documentation.Basics.Domain;
using Examples.From.Documentation.Basics.Wrappers;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ClassNeverInstantiated.Global

namespace Examples.From.Documentation.Basics.Environments
{
    public class CustomEnvironment : DefaultEnvironment
    {
        public CustomEnvironment()
        {
            Update<InitializeIoc>(block =>
            {
                block.AddTransient<IGetUserById, GetUserById>();
                block.AddTransient<IUserRepository, MockedUserRepository>();
            });

            Wrap<EmptyTestWrapper>();
        }
    }
}