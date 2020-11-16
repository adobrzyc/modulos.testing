using System;
using Modulos.Testing;
using SimpleDomain.Db.Model;

// ReSharper disable All
namespace SimpleDomain.Tests
{
    public partial class V2
    {
        [ModelDefinition]
        public class RegularUsers
        {
            [InlineData(OperationKind.Delete)] public static User TedSalesman => V1.RegularUsers.TedSalesman;

            [InlineData] public static User RebeccaCold => new User
            {
                UserId = new Guid("495E4573-3A32-4250-AF8A-0E0D3807369C"),
                Name = "Rebecca",
                SName = "Cold",
                DepartmentId = V1.Departments.HR.DepartmentId
            };
        }
    }
}