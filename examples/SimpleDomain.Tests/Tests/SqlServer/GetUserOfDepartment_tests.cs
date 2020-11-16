using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Modulos.Testing;
using SimpleDomain.Logic;
using Xunit;

namespace SimpleDomain.Tests.SqlServer
{
    [Collection(nameof(V2) + nameof(SqlServer))]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public class GetUserOfDepartment_tests
    {
        private readonly SqlServerEnv<V2.Definition> _env;

        public GetUserOfDepartment_tests(SqlServerEnv<V2.Definition> env)
        {
            _env = env;
        }

        [Fact(Skip = "Only for manual execution.")]
        public async Task obtain_users_from_it_department()
        {
            await using var test = await _env.CreateTest<Test>();

            var functionality = test.Resolve<GetUsersOfDepartment>();

            var users = await functionality.Execute(V2.Departments.IT.DepartmentId)
                .ConfigureAwait(false);

            users.Should().HaveCount(2);
            users.All(e => e.Department != null).Should().BeTrue();
            users.All(e => e.DepartmentId == V2.Departments.IT.DepartmentId).Should().BeTrue();
        }
    }
}