// ReSharper disable ClassNeverInstantiated.Global

namespace Examples.From.Documentation.Basics.Environments
{
    using Blocks;
    using Domain;
    using Microsoft.Extensions.DependencyInjection;
    using Wrappers;

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