// ReSharper disable UseAwaitUsing
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Modulos.Testing.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class TestEnvironment_Wrap
    {
        [Fact]
        public void check_arguments()
        {
            var env = new TestEnvironment();

            new Action(() => { env.Wrap(typeof(object)); }).Should().Throw<ArgumentException>();
            new Action(() => { env.Wrap(null); }).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void wrap_generic()
        {
            var env = new TestEnvironment();
            env.Wrap<Wrapper>();
            env.wrappers.Contains(typeof(Wrapper)).Should().BeTrue();
        }

        [Fact]
        public void wrap()
        {
            var env = new TestEnvironment();
            env.Wrap(typeof(Wrapper));
            env.wrappers.Contains(typeof(Wrapper)).Should().BeTrue();
        }

        [Fact]
        public async Task check_wrapper_execution()
        {
            var env = new TestEnvironment();
            env.Wrap(typeof(Wrapper));

            await env.Build();

            using (await env.CreateTest())
            {
                Wrapper.Counter.Should().Be(1);
            }

            Wrapper.Counter.Should().Be(0);
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