// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Modulos.Testing.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class pipeline_operations_tests
    {
        [Fact]
        public Task add_new_block()
        {
            var hostBuilder = new TestEnvironment()
                .Add<ExecuteLogic>((block, builder) => { });

            hostBuilder.IndexOf<ExecuteLogic>().Should().Be(0);

            return Task.CompletedTask;
        }

        [Fact]
        public async Task break_pipeline()
        {
            var output = new List<string>();
            var env = new TestEnvironment();
            env.Add<ExecuteLogic>((block, builder) => { block.Logic = () => { output.Add("1 message"); }; })
                .Add<BreakBlock>()
                .Add<ExecuteLogic>(Guid.NewGuid().ToString(), (block, builder) => { block.Logic = () => { output.Add("2 message"); }; });

            await env.Build();
            await env.CreateTest();

            output.Contains("1 message").Should().BeTrue();
            output.Contains("2 message").Should().BeFalse();
        }

        [Fact]
        public async Task insert_into_pipeline()
        {
            var output = new List<string>();
            var mark = Guid.NewGuid().ToString();

            var env = new TestEnvironment();
            env.Add<ExecuteLogic>((block, builder) => { block.Logic = () => { output.Add("1 message"); }; })
                .Add<ExecuteLogic>(mark, (block, builder) => { block.Logic = () => { output.Add("2 message"); }; });

            env.Insert<ExecuteLogic, BreakBlock>(InsertMode.Before, mark);

            await env.Build();
            await env.CreateTest();

            output.Contains("1 message").Should().BeTrue();
            output.Contains("2 message").Should().BeFalse();
        }

        [Fact]
        public async Task update_pipeline()
        {
            var output = new List<string>();
            var mark = Guid.NewGuid().ToString();

            var env = new TestEnvironment();
            env.Add<ExecuteLogic>((block, builder) => { block.Logic = () => { output.Add("1 message"); }; })
                .Add<ExecuteLogic>(mark, (block, builder) => { block.Logic = () => { output.Add("2 message"); }; });


            env.Update<ExecuteLogic>(mark, (block, builder) => { block.Logic = () => { output.Add("3 message"); }; });

            await env.Build();
            await env.CreateTest();

            output.Contains("1 message").Should().BeTrue();
            output.Contains("2 message").Should().BeFalse();
            output.Contains("3 message").Should().BeTrue();
        }

        // blocks must be public
        public class BreakBlock : IBlock
        {
            Task<BlockExecutionResult> IBlock.Execute(ITestEnvironment testEnv)
            {
                return Task.FromResult(BlockExecutionResult.EmptyBreak);
            }

            public Task ExecuteAtTheEnd(ITestEnvironment testEnv)
            {
                return Task.CompletedTask;
            }
        }

        public class ExecuteLogic : IBlock
        {
            public Action Logic { get; set; }

            Task<BlockExecutionResult> IBlock.Execute(ITestEnvironment testEnv)
            {
                Logic();
                return Task.FromResult(BlockExecutionResult.EmptyContinue);
            }

            public Task ExecuteAtTheEnd(ITestEnvironment testEnv)
            {
                return Task.CompletedTask;
            }
        }
    }
}