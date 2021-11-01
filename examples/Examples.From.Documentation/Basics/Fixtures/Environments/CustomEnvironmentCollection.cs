namespace Examples.From.Documentation.Basics.Environments
{
    using Xunit;

    [CollectionDefinition(nameof(CustomEnvironment))]
    public class CustomEnvironmentCollection : ICollectionFixture<CustomEnvironment>
    {
    }
}