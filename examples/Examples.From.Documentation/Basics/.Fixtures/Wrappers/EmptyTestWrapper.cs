using System.Threading.Tasks;
using Modulos.Testing;

// ReSharper disable UnusedType.Global

namespace Examples.From.Documentation.Basics.Wrappers
{
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