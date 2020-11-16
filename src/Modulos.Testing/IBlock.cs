using System.Threading.Tasks;

namespace Modulos.Testing
{
    /// <summary>
    /// Defines block executed via <see cref="ITestEnvironment"/>.
    /// It's important that block must be defined as a public class.
    /// </summary>
    public interface IBlock
    {
        /// <summary>
        /// Executes block logic.
        /// </summary>
        /// <param name="testEnv">
        /// Instance of <see cref="ITestEnvironment"/>.
        /// </param>
        /// <returns>
        /// Action to perform after block execution and additional data available for further blocks.
        /// </returns>
        /// <remarks>
        /// Any of executed blocks is available as dependency in any further block.
        /// </remarks>
        Task<BlockExecutionResult> Execute(ITestEnvironment testEnv);

        /// <summary>
        /// Executes when <see cref="ITestEnvironment"/> is disposed.
        /// </summary>
        /// <param name="testEnv"><see cref="ITestEnvironment"/> instance.</param>
        Task ExecuteAtTheEnd(ITestEnvironment testEnv);
    }
}