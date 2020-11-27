using System.Threading.Tasks;

// ReSharper disable ClassNeverInstantiated.Global

namespace Modulos.Testing.Tests
{
    public class Block : IBlock
    {
        public string Property { get; set; }

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