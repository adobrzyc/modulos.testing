using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Modulos.Testing;

namespace SimpleDomain.Tests.Blocks
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class InitializeIoc : IBlock
    {
        public Action<IServiceCollection, ContainerBuilder> RegisterServices { get; set; }
       
        Task<BlockExecutionResult> IBlock.Execute(ITestEnvironment testEnv)
        {
            var builder = new ContainerBuilder();
            var sc = new ServiceCollection();

            RegisterServices?.Invoke(sc, builder);
            builder.Populate(sc);
          
            var sp = new AutofacServiceProvider(builder.Build());

            testEnv.SetServiceProvider(sp);

            //// todo: consider if it's good idea
            //// make all registration available for further blocks just via DI
            //foreach (var serviceDescriptor in sc)
            //{
            //    outputServices.Add(serviceDescriptor);
            //}

            return Task.FromResult(BlockExecutionResult.EmptyContinue);
        }

        public Task ExecuteAtTheEnd(ITestEnvironment testEnv)
        {
            return Task.CompletedTask;
        }
    }
}