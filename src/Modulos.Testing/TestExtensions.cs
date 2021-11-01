// ReSharper disable UnusedType.Global

namespace Modulos.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;

    public static class TestExtensions
    {
        public static T Resolve<T>(this ITest test)
        {
            var result = test.GetService<T>();
            if (result != null)
                return result;

            return (T)Resolve(test, typeof(T));
        }

        private static object Resolve(IServiceProvider serviceProvider, Type typeToResolve, params object[] availableData)
        {
            ConstructorInfo ctor;
            try
            {
                ctor = typeToResolve.GetConstructors()
                    .Select(e => (ctor: e, count: e.GetParameters().Length))
                    .OrderByDescending(e => e.count)
                    .Select(e => e.ctor)
                    .First();
            }
            catch
            {
                return serviceProvider.GetRequiredService(typeToResolve);
            }

            var @params = availableData.ToDictionary(e => e.GetType(), e => e);

            var parameters = new List<object>();
            foreach (var paramInfo in ctor.GetParameters())
            {
                var keyFromAdditional = @params.Keys
                    .LastOrDefault(e => paramInfo.ParameterType.IsAssignableFrom(e));

                if (keyFromAdditional != null)
                {
                    var paramFromAdditional = @params[keyFromAdditional];

                    if (paramFromAdditional != null)
                    {
                        parameters.Add(paramFromAdditional);
                        continue;
                    }
                }

                if ((paramInfo.Attributes & ParameterAttributes.Optional) != 0)
                {
                    var value = serviceProvider.GetService(paramInfo.ParameterType);
                    parameters.Add(value);
                }
                else
                {
                    try
                    {
                        var value = serviceProvider.GetRequiredService(paramInfo.ParameterType);
                        parameters.Add(value);
                    }
                    catch (Exception e)
                    {
                        throw new Exception
                        (
                            $"Unable to resolve object: {typeToResolve.FullName} " +
                            $"parameter: {paramInfo.Name} of type {paramInfo.ParameterType.FullName}."
                            , e
                        );
                    }
                }
            }

            return ctor.Invoke(parameters.ToArray());
        }
    }
}