// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Modulos.Testing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class TestOptions : ITestOptions
    {
        private readonly ConcurrentBag<Type> wrappers = new();

        public ITestOptions Wrap<T>() where T : ITestWrapper
        {
            return Wrap(typeof(T));
        }

        public ITestOptions Wrap(Type wrapperType)
        {
            if (wrapperType == null) throw new ArgumentNullException(nameof(wrapperType));

            if (!typeof(ITestWrapper).IsAssignableFrom(wrapperType))
                throw new ArgumentException($"Wrapper must inherit from {nameof(ITestWrapper)}");

            if (wrappers.All(e => e != wrapperType))
                wrappers.Add(wrapperType);

            return this;
        }

        public ITestOptions Unwrap(Type wrapperType)
        {
            if (wrapperType == null) throw new ArgumentNullException(nameof(wrapperType));

            if (!typeof(ITestWrapper).IsAssignableFrom(wrapperType))
                throw new ArgumentException($"Wrapper must inherit from {nameof(ITestWrapper)}");

            wrappers.TryTake(out wrapperType);

            return this;
        }

        public ITestOptions Unwrap<T>()
        {
            return Unwrap(typeof(T));
        }

        public IEnumerable<Type> GetWrappers()
        {
            return wrappers;
        }
    }
}