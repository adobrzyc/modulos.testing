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
    public class TestEnvironment_Update
    {
        [Fact]
        public void check_arguments()
        {
            var env = new TestEnvironment();
            new Action(() => { env.UpdateInternal(mark: null,(Action<Block, ITestEnvironment>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Update(mark: null,(Action<Block, ITestEnvironment>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Update(mark: "mark",(Action<Block, ITestEnvironment>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Update((Action<Block, ITestEnvironment>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Update(mark: null,(Action<Block>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Update(mark: "mark",(Action<Block>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Update((Action<Block>) null); }).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task mark_setup_env()
        {
            var env = new TestEnvironment();
            env.Add<Block>("mark");

            env.Update<Block>("mark", (block, testEnv) =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                testEnv.Should().NotBeNull();
                block.Property = "a";
            });

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
            env.Add<Block>("mark");

            env.Update<Block>("mark", block =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
            });

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
        public async Task setup_env()
        {
            var env = new TestEnvironment();
            env.Add<Block>();

            env.Update<Block>((block, testEnv) =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                testEnv.Should().NotBeNull();
                block.Property = "a";
            });

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
            env.Add<Block>();

            env.Update<Block>(block =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
            });

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
        public async Task chain_update()
        {
            var env = new TestEnvironment();
            env.Add<Block>();

            env.Update<Block>(block =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
            });

            env.Update<Block>(block =>
            {
                block.Property.Should().Be("a");
                block.Property += "b";
            });

            env.Update<Block>(block =>
            {
                block.Property.Should().Be("ab");
                block.Property += "c";
            });

            var reg = env.registrations.FirstOrDefault();
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().BeNull();

            await env.Build();

            var inst = (Block) env.blocks.FirstOrDefault();
            inst.Should().NotBeNull();
            inst.Property.Should().Be("abc");
        }
    }
}