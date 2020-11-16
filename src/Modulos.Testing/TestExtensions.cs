using Microsoft.Extensions.DependencyInjection;
// ReSharper disable UnusedType.Global

namespace Modulos.Testing
{
    public static class TestExtensions
    {
        public static T Resolve<T>(this ITest test)
        {
            return test.ServiceProvider.GetRequiredService<T>();
        }
    }
}