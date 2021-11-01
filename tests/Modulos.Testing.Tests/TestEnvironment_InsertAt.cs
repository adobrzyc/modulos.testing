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

    public class TestEnvironment_InsertAt
    {
        [Fact]
        public void check_arguments()
        {
            var env = new TestEnvironment();

            new Action(() => { env.InsertAtInternal<Block>(-1, null, null); }).Should().Throw<ArgumentOutOfRangeException>();
            new Action(() => { env.InsertAt(0, (Action<Block, ITestEnvironment>)null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.InsertAt(0, (Action<Block>)null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.InsertAt(0, null, (Action<Block, ITestEnvironment>)null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.InsertAt(0, "mark", (Action<Block, ITestEnvironment>)null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.InsertAt(0, null, (Action<Block>)null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.InsertAt(0, "mark", (Action<Block>)null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.InsertAt<Block>(0, mark: null); }).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void invalid_index()
        {
            new Action(() =>
            {
                var env = new TestEnvironment();
                env.InsertAt<Block2>(1);
            }).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task empty_idx_0()
        {
            var env = new TestEnvironment();
            env.InsertAt<Block2>(0);

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().BeNull();

            (env.registrations.First().BlockType == typeof(Block2)).Should().BeTrue();

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().BeNull();
        }

        [Fact]
        public async Task empty_idx_0_v2()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.InsertAt<Block2>(0);

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().BeNull();

            (env.registrations.First().BlockType == typeof(Block2)).Should().BeTrue();

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().BeNull();
        }

        [Fact]
        public async Task empty_idx_1()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.InsertAt<Block2>(1);

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().BeNull();

            (env.registrations.ToArray()[1].BlockType == typeof(Block2)).Should().BeTrue();

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().BeNull();
        }

        [Fact]
        public async Task empty_idx_1_setup_env()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.InsertAt<Block2>(1, (block2, environment) =>
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

            (env.registrations.ToArray()[1].BlockType == typeof(Block2)).Should().BeTrue();

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task empty_idx_1_setup()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.InsertAt<Block2>(1, block2 =>
            {
                block2.Should().BeOfType<Block2>();
                block2.Should().NotBeNull();
                block2.Property = "a";
            });

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().BeNull();

            (env.registrations.ToArray()[1].BlockType == typeof(Block2)).Should().BeTrue();

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task mark_idx_1()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.InsertAt<Block2>(1, "mark");

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().Be("mark");

            (env.registrations.ToArray()[1].BlockType == typeof(Block2)).Should().BeTrue();

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().BeNull();
        }

        [Fact]
        public async Task mark_idx_1_setup_env()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.InsertAt<Block2>(1, "mark", (block2, environment) =>
            {
                block2.Should().BeOfType<Block2>();
                block2.Should().NotBeNull();
                environment.Should().NotBeNull();
                block2.Property = "a";
            });

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().Be("mark");

            (env.registrations.ToArray()[1].BlockType == typeof(Block2)).Should().BeTrue();

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task mark_idx_1_setup()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.InsertAt<Block2>(1, "mark", block2 =>
            {
                block2.Should().BeOfType<Block2>();
                block2.Should().NotBeNull();
                block2.Property = "a";
            });

            var reg = env.registrations.SingleOrDefault(e => e.BlockType == typeof(Block2));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block2>();
            reg.Mark.Should().Be("mark");

            (env.registrations.ToArray()[1].BlockType == typeof(Block2)).Should().BeTrue();

            await env.Build();

            var inst = (Block2)env.blocks.SingleOrDefault(e => e is Block2);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }
    }
}