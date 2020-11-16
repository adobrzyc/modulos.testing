using System;
using Modulos.Testing;
using SimpleDomain.Db.Model;

// ReSharper disable All
namespace SimpleDomain.Tests
{
    public partial class V1
    {
        [ModelDefinition]
        public class RegularUsers
        {
            [InlineData] public static User JohnnyManager => new User
            {
                UserId = new Guid("C3670250-6646-43D9-8D2E-199C2D71FF5A"),
                Name = "Johnny",
                SName = "Manager",
                DepartmentId = Departments.Management.DepartmentId
            };

            [InlineData] public static User TedSalesman => new User
            {
                UserId = new Guid("D4DD47DA-C452-4C03-A68D-01D437024093"),
                Name = "Ted",
                SName = "Salesman",
                DepartmentId = Departments.Sales.DepartmentId
            };

            [InlineData] public static User SaraHire => new User
            {
                UserId = new Guid("208EF9E9-9926-46FF-AB9F-93294FA29E44"),
                Name = "Sara",
                SName = "Hire",
                DepartmentId = Departments.HR.DepartmentId
            };
        }
    }
}