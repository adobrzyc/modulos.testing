using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CSharp.RuntimeBinder;
using Xunit;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable IdentifierTypo
// ReSharper disable ClassNeverInstantiated.Local

namespace Modulos.Testing.Tests
{
    public class TestEnvironment_Build
    {
        [Fact]
        public async Task private_blocks_are_not_supported_FORBIDDEN()
        {
            try
            {
                var env = new TestEnvironment();
                env.Add<PrivBlock>();
                await env.Build();
            }
            catch (AggregateException e)
            {
                e.InnerException.Should().BeOfType<RuntimeBinderException>();
                return;
            }

            throw new Exception("Missing exception.");
        }

        [Fact]
        public async Task with_extra_data()
        {
            var env = new TestEnvironment();
            env.Add<BlockWithData>();
            await env.Build("hello");
            var block = (BlockWithData) env.blocks.Single(e => e is BlockWithData);
            block.Data.Should().Be("hello");
        }

        [Fact]
        public async Task block_access_to_executed_block()
        {
            var env = new TestEnvironment();
            env.Add<BlockWithData>();
            env.Add<BlockAccesToBlock>();
            await env.Build("hello");
            var block = (BlockAccesToBlock) env.blocks.Single(e => e is BlockAccesToBlock);
            block.Block.Should().NotBeNull();
            block.Block.Data.Should().Be("hello");
        }

        [Fact]
        public async Task block_consume_data_produced_by_executed_block()
        {
            var env = new TestEnvironment();
            env.Add<BlockProducesDateTime>();
            env.Add<BlockConsumeDateTime>();
            await env.Build();
            var block = (BlockConsumeDateTime) env.blocks.Single(e => e is BlockConsumeDateTime);
            block.Time.Should().Be(DateTime.Today);
        }


        private class PrivBlock : IBlock
        {
            public Task<BlockExecutionResult> Execute(ITestEnvironment testEnv)
            {
                return Task.FromResult(BlockExecutionResult.EmptyContinue);
            }

            public Task ExecuteAtTheEnd(ITestEnvironment testEnv)
            {
                return Task.CompletedTask;
            }
        }

        public class BlockProducesDateTime : IBlock
        {
            Task<BlockExecutionResult> IBlock.Execute(ITestEnvironment testEnv)
            {
                return Task.FromResult(BlockExecutionResult.NewContinue(DateTime.Today));
            }

            Task IBlock.ExecuteAtTheEnd(ITestEnvironment testEnv)
            {
                return Task.CompletedTask;
            }
        }

        public class BlockConsumeDateTime : IBlock
        {
            public DateTime Time { get; }

            public BlockConsumeDateTime(DateTime time)
            {
                Time = time;
            }

            Task<BlockExecutionResult> IBlock.Execute(ITestEnvironment testEnv)
            {
                return Task.FromResult(BlockExecutionResult.EmptyContinue);
            }

            Task IBlock.ExecuteAtTheEnd(ITestEnvironment testEnv)
            {
                return Task.CompletedTask;
            }
        }


        public class BlockAccesToBlock : IBlock
        {
            public BlockWithData Block { get; }

            public BlockAccesToBlock(BlockWithData block)
            {
                Block = block;
            }

            Task<BlockExecutionResult> IBlock.Execute(ITestEnvironment testEnv)
            {
                return Task.FromResult(BlockExecutionResult.EmptyContinue);
            }

            Task IBlock.ExecuteAtTheEnd(ITestEnvironment testEnv)
            {
                return Task.CompletedTask;
            }
        }

        public class BlockWithData : IBlock
        {
            public string Data { get; }

            public BlockWithData(string data)
            {
                Data = data;
            }

            Task<BlockExecutionResult> IBlock.Execute(ITestEnvironment testEnv)
            {
                return Task.FromResult(BlockExecutionResult.EmptyContinue);
            }

            Task IBlock.ExecuteAtTheEnd(ITestEnvironment testEnv)
            {
                return Task.CompletedTask;
            }
        }

    }
}