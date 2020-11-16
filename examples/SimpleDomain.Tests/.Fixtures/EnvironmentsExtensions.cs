using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Modulos.Testing;
using SimpleDomain.Tests.Blocks;

namespace SimpleDomain.Tests
{
    public static class EnvironmentsExtensions
    {
        public static ITestEnvironment UpdateIoc(this ITestEnvironment env, Action<IServiceCollection, ContainerBuilder> update)
        {
            if(env.IndexOf<InitializeIoc>()<0)
                throw new NotSupportedException($"Specific env: {env.GetType().Name} does not contains: {nameof(InitializeIoc)} block.");

            env.Update<InitializeIoc>((block, environment, prevSetup) =>
            {
                prevSetup(block);
                var old = block.RegisterServices;
                block.RegisterServices = (collection, builder) =>
                {
                    old(collection, builder);
                    update(collection, builder);
                };
            });
            return env;
        }

        public static ITestEnvironment UpdateIoc(this ITestEnvironment env, Action<ContainerBuilder> update)
        {
            if(env.IndexOf<InitializeIoc>()<0)
                throw new NotSupportedException($"Specific env: {env.GetType().Name} does not contains: {nameof(InitializeIoc)} block.");

            env.Update<InitializeIoc>((block, environment, prevSetup) =>
            {
                prevSetup(block);
                var old = block.RegisterServices;
                block.RegisterServices = (collection, builder) =>
                {
                    old(collection, builder);
                    update(builder);
                };
            });
            return env;
        }

        public static ITestEnvironment UpdateIoc(this ITestEnvironment env, Action<IServiceCollection> update)
        {
            if(env.IndexOf<InitializeIoc>()<0)
                throw new NotSupportedException($"Specific env: {env.GetType().Name} does not contains: {nameof(InitializeIoc)} block.");

            env.Update<InitializeIoc>((block, environment, prevSetup) =>
            {
                prevSetup(block);
                var old = block.RegisterServices;
                block.RegisterServices = (collection, builder) =>
                {
                    old(collection, builder);
                    update(collection);
                };
            });
            return env;
        }
    }
}