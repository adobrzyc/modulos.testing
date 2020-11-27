using System.Collections.Generic;

// ReSharper disable MemberCanBePrivate.Global

namespace Modulos.Testing
{
    public class BlockExecutionResult
    {
        public static readonly BlockExecutionResult EmptyContinue = new BlockExecutionResult(ActionAfterBlock.Continue);
        public static readonly BlockExecutionResult EmptyBreak= new BlockExecutionResult(ActionAfterBlock.Break);

        public ActionAfterBlock Action { get; }
        public IEnumerable<object> PublishedData { get; }

        public static BlockExecutionResult NewContinue(params object[] data)
        {
            return new BlockExecutionResult(ActionAfterBlock.Continue, data);
        }

        public static BlockExecutionResult NewBreak(params object[] data)
        {
            return new BlockExecutionResult(ActionAfterBlock.Break, data);
        }

        public BlockExecutionResult(ActionAfterBlock action, params object[] data)
        {
            Action = action;
            PublishedData = data;
        }
    }
}