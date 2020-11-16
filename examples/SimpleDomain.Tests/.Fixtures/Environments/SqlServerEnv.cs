using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modulos.Testing;
using Modulos.Testing.EF;
using SimpleDomain.Db;
using SimpleDomain.Modules;
using SimpleDomain.Tests.Blocks;
using SimpleDomain.Tests.Wrappers;
using Xunit;

namespace SimpleDomain.Tests
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class SqlServerEnv<TModel> : TestEnvironment, IAsyncLifetime
        where TModel : class 
    {
        public SqlServerEnv()
        {
            Add<InitializeIoc>((block, builder) =>
            {
                block.RegisterServices = (services, autofac)=>
                {
                    autofac.RegisterModule<RegisterDependencies>();

                    services.AddScoped<ISeedProvider, SeedProviderForEf<Context>>();

                    services.AddDbContext<Context>(options =>
                    {
                        options.UseSqlServer("Server=localhost;" +
                                             "Integrated Security=true;" +
                                             $"Initial Catalog={GetDbName()};");
                    });
                };
            });

            Add<DropAndCreateDb<Context, TModel>>((block, builder) =>
            {
                block.DropDbAtTheEnd = false;
            });

            Wrap<BeginRollbackTran<Context>>();
        }

        private static string GetDbName()
        {
            return "SimpleDomainDb." + string.Join('-',
                typeof(TModel).FullName?.Split('.').Last().Split('+').First()
                ?? Guid.NewGuid().ToString());
        }

        public async Task InitializeAsync()
        {
            await Build();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await DisposeAsync();
        }
    }
}