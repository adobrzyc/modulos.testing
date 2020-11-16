using Xunit;

// ReSharper disable All
namespace SimpleDomain.Tests
{
    public partial class V1 
    {
        public class Collections
        {
            [CollectionDefinition(nameof(V1)+nameof(SqlServer))]
            public class SqlServer:ICollectionFixture<SqlServerEnv<Definition>>
            {

            }

            [CollectionDefinition(nameof(V1)+nameof(SqLite))]
            public class SqLite:ICollectionFixture<SqLiteEnv<Definition>>
            {

            }
        }
    }
}