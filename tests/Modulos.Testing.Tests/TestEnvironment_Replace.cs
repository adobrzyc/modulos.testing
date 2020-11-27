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
    public class TestEnvironment_Replace
    { 
        [Fact]
        public void check_arguments()
        {
            var env = new TestEnvironment();

            new Action(() => { env.ReplaceInternal((Action<Block, ITestEnvironment>) null,null,null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Replace((Action<Block, ITestEnvironment>) null); }).Should().Throw<ArgumentNullException>();
            new Action(() => { env.Replace((Action<Block>) null); }).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task setup()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.Replace<Block>(block =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
            });


            var reg = env.registrations.SingleOrDefault(e=>e.BlockType == typeof(Block));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().BeNull();

            await env.Build();

            var inst = (Block) env.blocks.SingleOrDefault(e=>e is Block);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task setup_env()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.Replace<Block>((block, environment) =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                environment.Should().NotBeNull();
                block.Property = "a";
            });


            var reg = env.registrations.SingleOrDefault(e=>e.BlockType == typeof(Block));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().BeNull();

            await env.Build();

            var inst = (Block) env.blocks.SingleOrDefault(e=>e is Block);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task setup_markToAdd()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.Replace<Block>(block =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
            }, markToAdd: "mark");


            var reg = env.registrations.SingleOrDefault(e=>e.BlockType == typeof(Block));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().Be("mark");

            await env.Build();

            var inst = (Block) env.blocks.SingleOrDefault(e=>e is Block);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task setup_markToFind()
        {
            var env = new TestEnvironment();
            env.Add<Block>("mark");
            env.Replace<Block>(block =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
            }, markToFind: "mark");


            var reg = env.registrations.SingleOrDefault(e=>e.BlockType == typeof(Block));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().BeNull();

            await env.Build();

            var inst = (Block) env.blocks.SingleOrDefault(e=>e is Block);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task setup_markToFind_markToAdd()
        {
            var env = new TestEnvironment();
            env.Add<Block>("mark");
            env.Replace<Block>(block =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
            }, markToFind: "mark", markToAdd: "mark2");


            var reg = env.registrations.SingleOrDefault(e=>e.BlockType == typeof(Block));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().Be("mark2");

            await env.Build();

            var inst = (Block) env.blocks.SingleOrDefault(e=>e is Block);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task setup_env_markToAdd()
        {
            var env = new TestEnvironment();
            env.Add<Block>();
            env.Replace<Block>((block,environment) =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
                environment.Should().NotBeNull();
            }, markToAdd: "mark");


            var reg = env.registrations.SingleOrDefault(e=>e.BlockType == typeof(Block));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().Be("mark");

            await env.Build();

            var inst = (Block) env.blocks.SingleOrDefault(e=>e is Block);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task setup_env__markToFind()
        {
            var env = new TestEnvironment();
            env.Add<Block>("mark");
            env.Replace<Block>((block,environment) =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
                environment.Should().NotBeNull();
            }, markToFind: "mark");


            var reg = env.registrations.SingleOrDefault(e=>e.BlockType == typeof(Block));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().BeNull();

            await env.Build();

            var inst = (Block) env.blocks.SingleOrDefault(e=>e is Block);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }

        [Fact]
        public async Task setup_env_markToFind_markToAdd()
        {
            var env = new TestEnvironment();
            env.Add<Block>("mark");
            env.Replace<Block>((block,environment) =>
            {
                block.Should().BeOfType<Block>();
                block.Should().NotBeNull();
                block.Property = "a";
                environment.Should().NotBeNull();
            }, markToFind: "mark", markToAdd: "mark2");


            var reg = env.registrations.SingleOrDefault(e=>e.BlockType == typeof(Block));
            reg.Should().NotBeNull();
            reg.BlockType.Should().Be<Block>();
            reg.Mark.Should().Be("mark2");

            await env.Build();

            var inst = (Block) env.blocks.SingleOrDefault(e=>e is Block);
            inst.Should().NotBeNull();
            inst.Property.Should().Be("a");
        }
    }
}