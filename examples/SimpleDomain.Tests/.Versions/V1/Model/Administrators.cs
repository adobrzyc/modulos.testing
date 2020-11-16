using System;
using Modulos.Testing;
using SimpleDomain.Db.Model;

// ReSharper disable All
namespace SimpleDomain.Tests
{
    public partial class V1
    {
        [ModelDefinition]
        public class Administrators
        {
            [InlineData]
            public static User SuperAdmin => new User
            {
                UserId = new Guid("47B4942F-C437-4A06-87DA-31AC819DE4B5"),
                Name = "Hacker",
                SName = "Cracker",
                DepartmentId = Departments.IT.DepartmentId
            };

            [InlineData]
            public static User Admin => new User
            {
                UserId = new Guid("01B92C9E-BFF2-4497-AF77-E025E22CC1D9"),
                Name = "Tom",
                SName = "System",
                DepartmentId = Departments.IT.DepartmentId
            };
        }
    }
}