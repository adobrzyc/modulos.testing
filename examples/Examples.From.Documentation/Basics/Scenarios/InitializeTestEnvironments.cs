// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Examples.From.Documentation.Basics.Scenarios
{
    using System.Threading.Tasks;
    using Blocks;
    using Domain;
    using Environments;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Modulos.Testing;
    using Xunit;

    /// <summary>
    /// This scenario shows different ways to create a test
    /// environment and control its lifetime.
    /// </summary>
    public class InitializeTestEnvironments
    {
        public static async Task SampleTest(ITestEnvironment env)
        {
            await using var test = await env.CreateTest();
            var functionality = test.Resolve<IGetUserById>();
            var result = functionality.Execute(0);
            result.Should().NotBeNull();
            result.Name.Should().Be("Tom");
        }

        /// <summary>
        /// Creating a brand new environment for each test method is very flexible,
        /// mainly to the possibility of reconfiguring blocks. For example it's possible
        /// to replace available registrations.
        /// </summary>
        public class Inline
        {
            [Fact]
            public async Task Test()
            {
                await using var env = new TestEnvironment();
                env.Add<InitializeIoc>(block =>
                {
                    block.AddSingleton<IUserRepository, MockedUserRepository>();
                    block.AddTransient<IGetUserById, GetUserById>();
                });
                await env.Build();

                await SampleTest(env);
            }
        }

        public class OneForClass : IClassFixture<CustomEnvironment>
        {
            private readonly CustomEnvironment env;

            public OneForClass(CustomEnvironment env)
            {
                this.env = env;
            }

            [Fact]
            public async Task Test()
            {
                await SampleTest(env);
            }
        }

        [Collection(nameof(CustomEnvironment))]
        public class FromCollection_1
        {
            private readonly CustomEnvironment env;

            public FromCollection_1(CustomEnvironment env)
            {
                this.env = env;
            }

            [Fact]
            public async Task Test()
            {
                await SampleTest(env);
            }
        }

        [Collection(nameof(CustomEnvironment))]
        public class FromCollection_2
        {
            private readonly CustomEnvironment env;

            public FromCollection_2(CustomEnvironment env)
            {
                this.env = env;
            }

            [Fact]
            public async Task Test()
            {
                await SampleTest(env);
            }
        }
    }
}