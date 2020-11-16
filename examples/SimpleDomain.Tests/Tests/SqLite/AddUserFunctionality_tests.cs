using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modulos.Testing;
using Modulos.Testing.EF;
using SimpleDomain.Db;
using SimpleDomain.Logic;
using Xunit;

namespace SimpleDomain.Tests.SqLite
{
    [Collection(nameof(V1) + nameof(SqLite))]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class AddUserFunctionality_tests
    {
        private readonly SqLiteEnv<V1.Definition> _env;

        public AddUserFunctionality_tests(SqLiteEnv<V1.Definition> env)
        {
            _env = env;
        }
        
        [Fact]
        public async Task add_non_existing_user()
        {
            await using var test = await _env.CreateTest<Test>();

            var functionality = test.Resolve<AddUserFunctionality>();
          
            var user = await functionality.CreateUser("John", "Good", V1.Departments.Sales.DepartmentId);
            user.Should().NotBeNull();
            user.Name.Should().Be("John");
            user.SName.Should().Be("Good");
            user.UserId.Should().NotBe(Guid.Empty);
            user.DepartmentId.Should().Be(V1.Departments.Sales.DepartmentId);
            user.Department.Should().BeNull();

            await test.AssertDb<Context>(async db =>
            {
                var toCheck = await db.Users.SingleOrDefaultAsync(e => e.UserId == user.UserId);

                toCheck.Should().NotBeNull();
                toCheck.Should().NotBeNull();
                toCheck.Name.Should().Be("John");
                toCheck.SName.Should().Be("Good");
                toCheck.UserId.Should().NotBe(Guid.Empty);
                toCheck.DepartmentId.Should().Be(V1.Departments.Sales.DepartmentId);
            });
        }

        [Fact]
        public async Task FORBIDDEN__add_user_with_null_name()
        {
            await using var test = await _env.CreateTest<Test>();
            try
            {
                var functionality = test.Resolve<AddUserFunctionality>();
                await functionality.CreateUser(null, "Good", V1.Departments.Sales.DepartmentId);
            }
            catch (ArgumentException ex)
            {
                ex.Should().NotBeNull();
                ex.ParamName.Should().Be("name");
                return;
            }

            throw new Exception($"Missing suspected exception: {nameof(ArgumentException)}");
        }

        [Fact]
        public async Task FORBIDDEN__add_user_with_null_sName()
        {
            await using var test = await _env.CreateTest<Test>();
            try
            {
                var functionality = test.Resolve<AddUserFunctionality>();
                await functionality.CreateUser("John", null, V1.Departments.Sales.DepartmentId);
            }
            catch (ArgumentException ex)
            {
                ex.Should().NotBeNull();
                ex.ParamName.Should().Be("sName");
                return;
            }

            throw new Exception($"Missing suspected exception: {nameof(ArgumentException)}");
        }

        [Fact]
        public async Task FORBIDDEN__add_existing_user()
        {
            await using var test = await _env.CreateTest<Test>();

            var user = V1.Administrators.Admin;

            try
            {
                var functionality = test.Resolve<AddUserFunctionality>();
                await functionality.CreateUser(user.Name, user.SName, V1.Departments.Sales.DepartmentId, user.UserId);
            }
            catch (DbUpdateException)
            {
                return;
            }

            throw new Exception($"Missing suspected exception: {nameof(DbUpdateException)}");

        }
    }
}
