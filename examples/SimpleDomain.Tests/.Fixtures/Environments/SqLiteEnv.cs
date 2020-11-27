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
    public class SqLiteEnv<TModel> : TestEnvironment, IAsyncLifetime
        where TModel : class
    {
        public SqLiteEnv()
        {
            Add<InitializeIoc>(block =>
            {
                block.Autofac.RegisterModule<RegisterDependencies>();
                block.AddScoped<ISeedProvider, SeedProviderForEf<Context>>();
                block.AddDbContext<Context>(options =>
                {
                    options.UseSqlite($"Data Source={GetDbName()}.db");
                });

                //todo: cleanup
                //block.RegisterServices = (services, autofac)=>
                //{
                //    autofac.RegisterModule<RegisterDependencies>();

                //    services.AddScoped<ISeedProvider, SeedProviderForEf<Context>>();

                //    services.AddDbContext<Context>(options =>
                //    {
                //        options.UseSqlite($"Data Source={GetDbName()}.db");
                //    });
                //};
            });

            Add<DropAndCreateDb<Context, TModel>>((block, evn) =>
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