using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Modulos.Testing;

namespace SimpleDomain.Tests.Wrappers
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class BeginRollbackTran<TContext> : ITestWrapper
        where TContext : DbContext
    {
        private readonly TContext _db;
        private IDbContextTransaction _transaction;

        public BeginRollbackTran(TContext db)
        {
            _db = db;
        }

        public async Task Begin()
        {
            if (_db.Database.CurrentTransaction == null)
                _transaction = await _db.Database.BeginTransactionAsync();
        }

        public async Task Finish()
        {
            if (_transaction != null)
            {
                await _transaction?.RollbackAsync();
                await _transaction.DisposeAsync();
            }
        }
    }
}