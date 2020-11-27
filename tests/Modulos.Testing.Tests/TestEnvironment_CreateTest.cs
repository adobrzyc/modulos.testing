using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Modulos.Testing.Tests
{
    public class TestEnvironment_CreateTest
    {
        [Fact]
        public async Task when_not_built_FORBIDDEN()
        {
            try
            {
                var env = new TestEnvironment();
                await env.CreateTest();
            }
            catch (ApplicationException)
            {
                return;
            }

            throw new Exception("Missing exception.");
        }

        [Fact]
        public async Task resolve_not_registered_non_optional_FORBIDDEN()
        {
            try
            {
                var env = new TestEnvironment();
                await env.Build();
                await env.CreateTest<ResolveNonRegisteredNonOptionalTest>();
            }
            catch (ArgumentException)
            {
                return;
            }

            throw new Exception("Missing exception.");
        }

        [Fact]
        public async Task resolve_ITestEnvironment_from_custom_test()
        {
            var env = new TestEnvironment();
            await env.Build();
            await env.CreateTest<ResolveEnvironmentTest>();
        }

        [Fact]
        public async Task resolve_optional()
        {
            var env = new TestEnvironment();
            await env.Build();
            await env.CreateTest<ResolveOptionalTest>();
        }

        private class ResolveEnvironmentTest : Test
        {
            public ResolveEnvironmentTest(IServiceProvider serviceProvider, TestOptions options, 
                ITestEnvironment environment) : base(serviceProvider, options)
            {
                environment.Should().NotBeNull();
            }
        }

        private interface ISomeNonRegisteredData {}

        private class ResolveOptionalTest : Test
        {
            public ResolveOptionalTest(IServiceProvider serviceProvider, TestOptions options, 
                [Optional] ISomeNonRegisteredData data) : base(serviceProvider, options)
            {
                data.Should().BeNull();
            }
        }

        private class ResolveNonRegisteredNonOptionalTest: Test
        {
            public ResolveNonRegisteredNonOptionalTest(IServiceProvider serviceProvider, TestOptions options, 
                ISomeNonRegisteredData data) : base(serviceProvider, options)
            {
                data.Should().BeNull();
            }
        }
    }
}