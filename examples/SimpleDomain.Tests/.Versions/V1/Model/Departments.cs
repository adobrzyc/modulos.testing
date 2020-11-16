using System;
using Modulos.Testing;
using SimpleDomain.Db.Model;

// ReSharper disable All
namespace SimpleDomain.Tests
{
    public partial class V1
    {
        [ModelDefinition]
        public class Departments
        {
            [InlineData] public static Department IT => new Department
            {
                DepartmentId = new Guid("91E11AD8-C3A5-4E3D-B04A-D830D0E77B6E"),
                Name = $"{nameof(IT)}"
            };

            [InlineData] public static Department Management => new Department
            {
                DepartmentId = new Guid("4AD0B23B-853F-43F4-8CFE-680CFC57A82F"),
                Name = $"{nameof(Management)}"
            };

            [InlineData] public static Department Sales => new Department
            {
                DepartmentId = new Guid("0E95D86A-5C96-4957-ADBD-6A8FD67E9B01"),
                Name = $"{nameof(Sales)}"
            };

            [InlineData] public static Department HR => new Department
            {
                DepartmentId = new Guid("5324AA2E-CF55-4E5C-88E3-EB2F327F6DC1"),
                Name = $"{nameof(HR)}"
            };
        }
    }
}