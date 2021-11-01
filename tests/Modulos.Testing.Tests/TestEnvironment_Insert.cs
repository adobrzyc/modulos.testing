// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Modulos.Testing.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class TestEnvironment_Insert
    {
        [Fact]
        public void check_arguments()
        {
            var env = new TestEnvironment();
            new Action(() => { env.Insert<Block, Block2>(InsertMode.After, (Action<Block2, ITestEnvironment>)null); })
                .Should().Throw<ArgumentNullException>();
            new Action(() => { env.Insert<Block, Block2>(InsertMode.After, (Action<Block2>)null); })
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void after_before()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.Insert<Block, Block2>(InsertMode.Before);

            env.IndexOf<Block>().Should().Be(1);
            env.IndexOf<Block2>().Should().Be(0);

            env = new TestEnvironment();
            env.Add<Block>();
            env.Insert<Block, Block2>(InsertMode.After);

            env.IndexOf<Block>().Should().Be(0);
            env.IndexOf<Block2>().Should().Be(1);
        }

        [Fact]
        public void markToFind()
        {
            var env = new TestEnvironment();
            env.Add<Block>("mark");
            env.Insert<Block, Block2>(InsertMode.Before, "mark");
            env.IndexOf<Block2>().Should().Be(0);
        }

        [Fact]
        public void markToInsert()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.Insert<Block, Block2>(InsertMode.Before, markToInsert: "mark");

            env.IndexOf<Block2>("mark").Should().Be(0);
            env.IndexOf<Block2>().Should().Be(-1);
        }


        [Fact]
        public async Task empty()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.Insert<Block, Block2>(InsertMode.Before);

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().BeNull();

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().BeNull();
        }

        [Fact]
        public async Task setup()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.Insert<Block, Block2>(InsertMode.Before, block2 =>
            {
                block2.Should().BeOfType<Block2>();
                block2.Should().NotBeNull();
                block2.Property = "a";
            });

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().BeNull();

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task setup_env()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.Insert<Block, Block2>(InsertMode.Before, (block2, environment) =>
            {
                block2.Should().BeOfType<Block2>();
                block2.Should().NotBeNull();
                environment.Should().NotBeNull();
                block2.Property = "a";
            });

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().BeNull();

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }


        [Fact]
        public async Task setup_marks_env()
        {
            var env = new TestEnvironment();
            env.Add<Block>("mark");
            env.Insert<Block, Block2>(InsertMode.Before, (block2, environment) =>
            {
                block2.Should().BeOfType<Block2>();
                block2.Should().NotBeNull();
                environment.Should().NotBeNull();
                block2.Property = "a";
            }, "mark", "mark2");

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().Be("mark2");

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task setup_marks()
        {
            var env = new TestEnvironment();
            env.Add<Block>("mark");
            env.Insert<Block, Block2>(InsertMode.Before, block2 =>
            {
                block2.Should().BeOfType<Block2>();
                block2.Should().NotBeNull();
                block2.Property = "a";
            }, "mark", "mark2");

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().Be("mark2");

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }
    }
}