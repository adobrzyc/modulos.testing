using System;
using System.Threading.Tasks;

namespace Modulos.Testing
{
    public static class TestExtensions
    {
        public static Task<TContext> SeedDb<TContext>(this ITest test, Func<ISeedProvider, Task> setup = null)
            where TContext : class
        {
            return SeedDb<TContext>(test, async (provider, context) =>
            {
                if(setup != null)
                    await setup(provider);
            });
        }

        public static async Task<TContext> SeedDb<TContext>(this ITest test, Func<ISeedProvider, TContext, Task> setup = null)
            where TContext : class
        {
            var seedProvider = test.Resolve<ISeedProvider>();
            var context = test.Resolve<TContext>();

            if (setup != null)
                await setup(seedProvider, context);

            await seedProvider.Seed();

            return context;
        }
    }
}