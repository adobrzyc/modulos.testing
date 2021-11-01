// ReSharper disable UnusedType.Global

namespace Examples.From.Documentation.Basics.Wrappers
{
    using System.Threading.Tasks;
    using Modulos.Testing;

    public class EmptyTestWrapper : ITestWrapper
    {
        public Task Begin()
        {
            return Task.CompletedTask;
        }

        public Task Finish()
        {
            return Task.CompletedTask;
        }
    }
}