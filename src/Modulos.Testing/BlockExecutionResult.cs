using System.Collections.Generic;

namespace Modulos.Testing
{
    public class BlockExecutionResult
    {
        public static readonly BlockExecutionResult EmptyContinue = new BlockExecutionResult(ActionAfterBlock.Continue);
        public static readonly BlockExecutionResult EmptyBreak= new BlockExecutionResult(ActionAfterBlock.Break);

        public ActionAfterBlock Action { get; }
        public IEnumerable<object> PublishedData { get; }

        public BlockExecutionResult(ActionAfterBlock action, params object[] data)
        {
            Action = action;
            PublishedData = data;
        }
    }
}