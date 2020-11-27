using System;
using System.Linq;
using FluentAssertions;
using Xunit;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Modulos.Testing.Tests
{
    public class TestEnvironment_Remove
    {
        [Fact]
        public void check_arguments()
        {
            var env = new TestEnvironment();
            new Action(() => { env.Remove<Block>(null); }).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void empty()
        {
            var env = new TestEnvironment();

            env.Add<Block>();
            env.Remove<Block>();

            env.registrations.Should().BeEmpty();
        }

        [Fact]
        public void empty_v2()
        {
            var env = new TestEnvironment();

            env.Add<Block>();
            env.Add<Block2>();
            env.Remove<Block>();

            env.registrations.Should().HaveCount(1);
            env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2))
                .Should().NotBeNull();
        }

        [Fact]
        public void mark()
        {
            var env = new TestEnvironment();

            env.Add<Block>("mark");
            env.Remove<Block>("mark");

            env.registrations.Should().BeEmpty();
        }

        [Fact]
        public void mark_v2()
        {
            var env = new TestEnvironment();

            env.Add<Block>("mark");
            env.Add<Block2>();
            env.Remove<Block>("mark");

            env.registrations.Should().HaveCount(1);
            env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2))
                .Should().NotBeNull();
        }
    }
}