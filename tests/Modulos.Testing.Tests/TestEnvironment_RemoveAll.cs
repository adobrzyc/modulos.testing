// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Modulos.Testing.Tests
{
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class TestEnvironment_RemoveAll
    {
        [Fact]
        public void single()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.RemoveAll<Block>();

            env.registrations.Should().BeEmpty();
        }

        [Fact]
        public void multiple()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.Add<Block2>();
            env.Add<Block>("mark1");
            env.Add<Block>("mark2");

            env.RemoveAll<Block>();

            env.registrations.All(e => e.BlockType != typeof(Block));
        }
    }
}