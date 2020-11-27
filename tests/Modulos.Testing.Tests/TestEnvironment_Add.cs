using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Modulos.Testing.Tests
{
    public class TestEnvironment_Add
    {
        [Fact]
        public void check_arguments()
        {
            var env = new TestEnvironment();

            new Action(() => { env.Add<Block>(mark: null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Add((Action<Block, ITestEnvironment>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Add(null,(Action<Block, ITestEnvironment>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Add("mark",(Action<Block, ITestEnvironment>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Add((Action<Block>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Add(null,(Action<Block>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Add("mark",(Action<Block>) null); }).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task mark_setup_env()
        {
            var env = new TestEnvironment();

            env.Add<Block>("mark", (block, testEnv) =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                testEnv.Should().NotBeNull();
                block.Property = "a";
            });

            env.IndexOf<Block>().Should().Be(-1);
            env.IndexOf<Block>("mark").Should().Be(0);

            var reg = env.registrations.FirstOrDefault();
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().Be("mark");

            await env.Build();

            var inst = (Block) env.blocks.FirstOrDefault();
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task mark_setup()
        {
            var env = new TestEnvironment();

            env.Add<Block>("mark", block =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
            });

            env.IndexOf<Block>().Should().Be(-1);
            env.IndexOf<Block>("mark").Should().Be(0);

            var reg = env.registrations.FirstOrDefault();
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().Be("mark");

            await env.Build();

            var inst = (Block) env.blocks.FirstOrDefault();
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task mark()
        {
            var env = new TestEnvironment();

            env.Add<Block>("mark");

            env.IndexOf<Block>().Should().Be(-1);
            env.IndexOf<Block>("mark").Should().Be(0);

            var reg = env.registrations.FirstOrDefault();
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().Be("mark");

            await env.Build();

            var inst = (Block) env.blocks.FirstOrDefault();
            inst.Should().NotBeNull();
            inst.Property.Should().BeNull();
        }

        [Fact]
        public async Task setup_env()
        {
            var env = new TestEnvironment();

            env.Add<Block>((block, testEnv) =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                testEnv.Should().NotBeNull();
                block.Property = "a";
            });

            env.IndexOf<Block>().Should().Be(0);
            env.IndexOf<Block>("mark").Should().Be(-1);

            var reg = env.registrations.FirstOrDefault();
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().BeNull();

            await env.Build();

            var inst = (Block) env.blocks.FirstOrDefault();
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task setup()
        {
            var env = new TestEnvironment();

            env.Add<Block>(block =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
            });

            env.IndexOf<Block>().Should().Be(0);
            env.IndexOf<Block>("mark").Should().Be(-1);

            var reg = env.registrations.FirstOrDefault();
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().BeNull();

            await env.Build();

            var inst = (Block) env.blocks.FirstOrDefault();
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task empty()
        {
            var env = new TestEnvironment();

            env.Add<Block>();

            env.IndexOf<Block>().Should().Be(0);
            env.IndexOf<Block>("mark").Should().Be(-1);

            var reg = env.registrations.FirstOrDefault();
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().BeNull();

            await env.Build();

            var inst = (Block) env.blocks.FirstOrDefault();
            inst.Should().NotBeNull();
            inst.Property.Should().BeNull();
        }

        [Fact]
        public void FORBIDDEN_same_block()
        {
            new Action(() =>
            {
                var env = new TestEnvironment();
                env.Add<Block>();
                env.Add<Block>();

            }).Should().Throw<ArgumentException>();

        }
    }
}