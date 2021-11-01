namespace Examples.From.Documentation.Basics.Scenarios
{
    using System.Threading.Tasks;
    using Modulos.Testing;

    public class Wrappers
    {
        public async Task WrapTest()
        {
            new TestEnvironment().Wrap<TestWrapper>();

            await using var test = await new TestEnvironment()
                .CreateTest(options => { options.Wrap<TestWrapper>(); });
        }

        private class TestWrapper : ITestWrapper
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
}