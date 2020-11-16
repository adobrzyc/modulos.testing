using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Modulos.Testing.EF.Tests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    public class SeedProvider_tests
    {
        [Fact]
        public void model_definition_included_in_particular_order()
        {
            var seed = new SeedProviderForEf<DbContext>(new Mock<DbContext>().Object);
            seed.Add<RootModel>();
            seed.Model.Should()
                .ContainInOrder
                (
                    new ModelDefinition(typeof(RootModel)),
                    new ModelDefinition(typeof(RootModel.NestedModel)),
                    new ModelDefinition(typeof(AnotherRootModel)),
                    new ModelDefinition(typeof(AnotherRootModel.AnotherNestedModel))
                );
        }

        [Fact]
        public void model_definition_included_in_particular_order_v2()
        {
            var seed = new SeedProviderForEf<DbContext>(new Mock<DbContext>().Object);
            seed.Add<Model1Model2>();
            seed.Model.Should()
                .ContainInOrder
                (
                    new ModelDefinition(typeof(Model1)),
                    new ModelDefinition(typeof(Model2))
                );
        }

       
        [ModelDefinition]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        private class Model1
        {
            [InlineData]
            public static User User1 => new User();
        }

        [ModelDefinition]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        private class Model2
        {
            [InlineData]
            public static User User1 => new User();
        }

        [ModelDefinition]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        private class Model1Model2
        {
            [IncludeModel]
            public static Type[] Include =
            {
                typeof(Model1),
                typeof(Model2)
            };
        }


        [ModelDefinition]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        private class RootModel
        {
            [InlineData]
            public static User User1 => new User();

            [ModelDefinition]
            public class NestedModel
            {
                [InlineData]
                public static User User2 => new User();
            }

            [IncludeModel]
            public static Type[] Include =
            {
                typeof(AnotherRootModel)
            };
        }

        [ModelDefinition]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        private class AnotherRootModel
        {
            [InlineData]
            public static User User3 => new User();

            [ModelDefinition]
            public class AnotherNestedModel
            {
                [InlineData]
                public static User User4 => new User();
            }
        }


        private class User
        {

        }
    }
}
