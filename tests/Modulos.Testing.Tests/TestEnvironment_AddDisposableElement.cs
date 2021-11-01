// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Modulos.Testing.Tests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class TestEnvironment_AddDisposableElement
    {
        [Fact]
        public void add_object()
        {
            var env = new TestEnvironment();
            var obj = new Disposable();
            env.AddDisposableElement(obj);
            env.disposables.Contains(obj)
                .Should().BeTrue();
        }

        private class Disposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}