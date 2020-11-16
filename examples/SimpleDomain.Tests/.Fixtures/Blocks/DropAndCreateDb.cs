using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modulos.Testing;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SimpleDomain.Tests.Blocks
{
    /// <summary>
    /// Drop already existing db and creates new one. 
    /// </summary>
    /// <typeparam name="TContext">Database context type.</typeparam>
    /// <typeparam name="TModel">Mode used to initialize database.</typeparam>
    public sealed class DropAndCreateDb<TContext, TModel> : IBlock
        where TContext : DbContext
        where TModel : class
    {
        private readonly TContext db;
        private readonly ISeedProvider seedProvider;

        public bool DropDbAtTheEnd { get; set; } = true;
        public bool RecreateDbAtStart { get; set; } = true;

        public DropAndCreateDb(TContext db, ISeedProvider seedProvider)
        {
            this.db = db;
            this.seedProvider = seedProvider;
        }

        async Task<BlockExecutionResult> IBlock.Execute(ITestEnvironment testEnv)
        {
            if (RecreateDbAtStart)
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
            }

            seedProvider.Add<TModel>();
            await seedProvider.Seed();

            return BlockExecutionResult.EmptyContinue;
        }

        public async Task ExecuteAtTheEnd(ITestEnvironment testEnv)
        {
            if (DropDbAtTheEnd)
                await db.Database.EnsureDeletedAsync();
        }
    }
}