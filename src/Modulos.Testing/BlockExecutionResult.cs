// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Modulos.Testing
{
    using System.Collections.Generic;

    /// <summary>
    /// Block execution result.
    /// </summary>
    public class BlockExecutionResult
    {
        public static readonly BlockExecutionResult EmptyContinue = new(ActionAfterExecution.Continue);
        public static readonly BlockExecutionResult EmptyBreak = new(ActionAfterExecution.Break);

        public BlockExecutionResult(ActionAfterExecution action, params object[] data)
        {
            Action = action;
            PublishedData = data;
        }

        public ActionAfterExecution Action { get; }
        public IEnumerable<object> PublishedData { get; }

        public static BlockExecutionResult NewContinue(params object[] data)
        {
            return new BlockExecutionResult(ActionAfterExecution.Continue, data);
        }

        public static BlockExecutionResult NewBreak(params object[] data)
        {
            return new BlockExecutionResult(ActionAfterExecution.Break, data);
        }
    }
}