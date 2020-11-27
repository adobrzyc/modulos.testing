using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedMember.Global

namespace Modulos.Testing
{
    public class TestOptions
    {
        private readonly ConcurrentQueue<Type> wrappers = new ConcurrentQueue<Type>();
        
        /// <summary>
        /// Allows to overwrite scope creation. It may be useful in some situations like
        /// overwriting registrations in a child scope (if possible e.q.: autofac).
        /// </summary>
        public Func<IServiceProvider, IServiceScope> CreateScope;

        public TestOptions Wrap<T>() where T : ITestWrapper
        {
            return Wrap(typeof(T));
        }

        public TestOptions Wrap(Type wrapper)
        {
            if (wrapper == null) throw new ArgumentNullException(nameof(wrapper));

            if (!typeof(ITestWrapper).IsAssignableFrom(wrapper))
                throw new ArgumentException($"Wrapper must inherit from {nameof(ITestWrapper)}");

            wrappers.Enqueue(wrapper);
            return this;
        }

        public IEnumerable<Type> GetWrappers()
        {
            return wrappers;
        }
    }
}