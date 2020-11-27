using Xunit;

namespace Examples.From.Documentation.Basics.Environments
{
    [CollectionDefinition(nameof(CustomEnvironment))]
    public class CustomEnvironmentCollection : ICollectionFixture<CustomEnvironment>
    {

    }
}