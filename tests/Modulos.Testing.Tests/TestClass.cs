using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
// ReSharper disable MemberCanBePrivate.Local

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming

namespace Modulos.Testing.Tests
{
    public class TestClass
    {
        [Fact]
        public async Task check_wrapper_execution()
        {
            await using var sp = new ServiceCollection().BuildServiceProvider();
            var test = new Test(sp, new TestOptions().Wrap<Wrapper>());

            Wrapper.Counter.Should().Be(0);
            await ((ITest) test).BeginWrappers();
            Wrapper.Counter.Should().Be(1);
            await test.DisposeAsync();
            Wrapper.Counter.Should().Be(0);

            await ((ITest) test).BeginWrappers();
            Wrapper.Counter.Should().Be(1);
            // ReSharper disable once MethodHasAsyncOverload
            test.Dispose();
            Wrapper.Counter.Should().Be(0);


            await ((ITest) test).BeginWrappers();
            Wrapper.Counter.Should().Be(1);
            // ReSharper disable once MethodHasAsyncOverload
            await ((ITest) test).FinishWrappers();
            Wrapper.Counter.Should().Be(0);

        }

        [Fact]
        public void GetService_method()
        {
            using var sp = new ServiceCollection()
                .AddTransient<Reference>()
                .BuildServiceProvider();

            using var test = new Test(sp, new TestOptions());
            test.GetService(typeof(Reference)).Should().NotBeNull();
        }

        [Fact]
        public async Task wrapper_with_reference_to_ITest()
        {
            await using var sp = new ServiceCollection().BuildServiceProvider();
            await using ITest test = new Test(sp, new TestOptions().Wrap<WrapperWithITest>());
            await test.BeginWrappers();
        }


        [Fact]
        public async Task wrapper_throws_error_during_finish()
        {
            await using var sp = new ServiceCollection().BuildServiceProvider();
            var test = new Test(sp, new TestOptions().Wrap<WrapperWithErrorOnFinish>());

            try
            {
                await ((ITest) test).BeginWrappers();
                await ((ITest) test).FinishWrappers();
            }
            catch
            {
                return;
            }

            throw new Exception("Missing exception.");
           
        }

        private class Reference
        {

        }

        private class WrapperWithITest : ITestWrapper
        {
            public WrapperWithITest(ITest test)
            {
                test.Should().NotBeNull();
            }

            public Task Begin()
            {
                return Task.CompletedTask;
            }

            public Task Finish()
            {
                return Task.CompletedTask;
            }
        }

        private class WrapperWithErrorOnFinish : ITestWrapper
        {
            public static int Counter;

            public Task Begin()
            {
                Interlocked.Increment(ref Counter);
                return Task.CompletedTask;
            }

            public Task Finish()
            {
                throw new Exception();
            }
        }

        private class Wrapper : ITestWrapper
        {
            public static int Counter;

            public Task Begin()
            {
                Interlocked.Increment(ref Counter);
                return Task.CompletedTask;
            }

            public Task Finish()
            {
                Interlocked.Decrement(ref Counter);
                return Task.CompletedTask;
            }
        }
    }
}