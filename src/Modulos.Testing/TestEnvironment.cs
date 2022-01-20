// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Modulos.Testing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.Extensions.DependencyInjection;

    public class TestEnvironment : ITestEnvironment
    {
        internal readonly ConcurrentBag<IBlock> blocks = new();
        internal readonly ConcurrentBag<object> disposables = new();
        private readonly object locker = new();

        internal readonly List<IBlockRegistration> registrations = new();
        internal readonly List<Type> wrappers = new();
        private bool isBuilt;
        private IServiceProvider serviceProvider;

        private bool IsBuilt
        {
            get
            {
                lock (locker)
                {
                    return isBuilt;
                }
            }
        }

        public async Task<ITestEnvironment> Build(params object[] data)
        {
            lock (locker)
            {
                if (isBuilt)
                    throw new ApplicationException("The test environment is already built.");
                isBuilt = true;
            }

            var parameters = new List<object>(data);
            try
            {
                foreach (var registration in registrations)
                {
                    var block = ResolveBlock(registration.BlockType, parameters.ToArray());

                    try
                    {
                        ((dynamic)registration).Setup?.Invoke((dynamic)block, (dynamic)this);
                    }
                    catch (RuntimeBinderException e)
                    {
                        throw new AggregateException($"Specified block: {registration.BlockType} is probably defined " +
                                                     "in inaccessible scope e.q.: private. Blocks should be defined as a public classes.", e);
                    }

                    var result = await block.Execute(this);
                    parameters.Add(block);
                    parameters.AddRange(result.PublishedData);
                    blocks.Add(block);

                    switch (result.Action)
                    {
                        case ActionAfterExecution.Continue:
                            continue;
                        case ActionAfterExecution.Break:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                }
            }
            finally
            {
                foreach (var param in parameters)
                    if (param is IAsyncDisposable or IDisposable)
                        disposables.Add(param);
            }

            return this;
        }


        public IServiceProvider GetServiceProvider()
        {
            lock (locker)
            {
                return serviceProvider ?? new ServiceCollection().BuildServiceProvider();
            }
        }

        public void SetServiceProvider(IServiceProvider provider)
        {
            lock (locker)
            {
                serviceProvider = provider;
            }
        }

        public virtual async Task<TTest> CreateTest<TTest, TOptions>(Action<TOptions> updateOptions = null)
            where TTest : ITest
            where TOptions : ITestOptions, new()
        {
            if (!IsBuilt)
                throw new ApplicationException($"Before call {nameof(CreateTest)}, {nameof(Build)} method must be called first.");

            var options = new TOptions();
            foreach (var wrapper in wrappers) options.Wrap(wrapper);
            updateOptions?.Invoke(options);

            var sp = GetServiceProvider();

            var ctor = typeof(TTest).GetConstructors()
                .Select(e => (ctor: e, count: e.GetParameters().Length))
                .OrderByDescending(e => e.count)
                .Select(e => e.ctor)
                .First();

            var parameters = new List<object>();

            foreach (var param in ctor.GetParameters())
            {
                if (param.ParameterType == typeof(ITestEnvironment))
                {
                    parameters.Add(this);
                    continue;
                }

                if (param.ParameterType == typeof(ITestOptions))
                {
                    parameters.Add(options);
                    continue;
                }
                
                if (param.ParameterType == typeof(TOptions))
                {
                    parameters.Add(options);
                    continue;
                }

                if ((param.Attributes & ParameterAttributes.Optional) != 0)
                    parameters.Add(sp.GetService(param.ParameterType));
                else
                    try
                    {
                        parameters.Add(sp.GetRequiredService(param.ParameterType));
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException
                        (
                            $"Unable to resolve test: {typeof(TTest).FullName} " +
                            $"parameter: {param.Name} of type {param.ParameterType.FullName}."
                            , e
                        );
                    }
            }

            var test = (TTest)Activator.CreateInstance(typeof(TTest), parameters.ToArray());

            await test.BeginWrappers();

            return test;
        }

     

        public Task<Test> CreateTest(Action<TestOptions> updateOptions = null)
        {
            return CreateTest<Test, TestOptions>(updateOptions);
        }

        public Task<TTest> CreateTest<TTest>(Action<TestOptions> updateOptions = null)
            where TTest : ITest
        {
            return CreateTest<TTest, TestOptions>(updateOptions);
        }
        
        public void AddDisposableElement(IDisposable disposable)
        {
            disposables.Add(disposable);
        }


        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }


        public ITestEnvironment Wrap<T>() where T : ITestWrapper
        {
            return Wrap(typeof(T));
        }

        public ITestEnvironment Wrap(Type wrapperType)
        {
            if (wrapperType == null) throw new ArgumentNullException(nameof(wrapperType));

            if (!typeof(ITestWrapper).IsAssignableFrom(wrapperType))
                throw new ArgumentException($"Wrapper must inherit from {nameof(ITestWrapper)}");

            wrappers.Add(wrapperType);

            return this;
        }


        public ITestEnvironment Add<TBlock>(string mark, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock
        {
            if (mark == null) throw new ArgumentNullException(nameof(mark));
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            return AddInternal(mark, setup);
        }

        public ITestEnvironment Add<TBlock>(string mark, Action<TBlock> setup)
            where TBlock : IBlock
        {
            if (mark == null) throw new ArgumentNullException(nameof(mark));
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var wrappedSetup = new Action<TBlock, ITestEnvironment>((block, _) => { setup(block); });

            return AddInternal(mark, wrappedSetup);
        }

        public ITestEnvironment Add<TBlock>(Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            return AddInternal(null, setup);
        }

        public ITestEnvironment Add<TBlock>(Action<TBlock> setup)
            where TBlock : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            var wrappedSetup = new Action<TBlock, ITestEnvironment>((block, _) => { setup(block); });

            return AddInternal(null, wrappedSetup);
        }

        public ITestEnvironment Add<TBlock>(string mark)
            where TBlock : IBlock
        {
            if (mark == null) throw new ArgumentNullException(nameof(mark));
            return AddInternal<TBlock>(mark, null);
        }

        public ITestEnvironment Add<TBlock>() where TBlock : IBlock
        {
            return AddInternal<TBlock>(null, null);
        }

        public ITestEnvironment InsertAt<TBlock>(int index, string mark, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock
        {
            if (mark == null) throw new ArgumentNullException(nameof(mark));
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            return InsertAtInternal(index, mark, setup);
        }

        public ITestEnvironment InsertAt<TBlock>(int index, string mark, Action<TBlock> setup)
            where TBlock : IBlock
        {
            if (mark == null) throw new ArgumentNullException(nameof(mark));
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var wrappedSetup = new Action<TBlock, ITestEnvironment>((block, _) => { setup(block); });

            return InsertAtInternal(index, mark, wrappedSetup);
        }

        public ITestEnvironment InsertAt<TBlock>(int index, string mark)
            where TBlock : IBlock
        {
            if (mark == null) throw new ArgumentNullException(nameof(mark));

            return InsertAtInternal<TBlock>(index, mark, null);
        }

        public ITestEnvironment InsertAt<TBlock>(int index, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            return InsertAtInternal(index, null, setup);
        }

        public ITestEnvironment InsertAt<TBlock>(int index, Action<TBlock> setup)
            where TBlock : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            var wrappedSetup = new Action<TBlock, ITestEnvironment>((block, _) => { setup(block); });

            return InsertAtInternal(index, null, wrappedSetup);
        }

        public ITestEnvironment InsertAt<TBlock>(int index)
            where TBlock : IBlock
        {
            return InsertAtInternal<TBlock>(index, null, null);
        }

        public ITestEnvironment Insert<TBlockToFind, TBlockToInsert>(InsertMode mode,
            Action<TBlockToInsert, ITestEnvironment> setup,
            string markToFind = null,
            string markToInsert = null)
            where TBlockToFind : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            return InternalInsert<TBlockToFind, TBlockToInsert>(mode, setup, markToFind, markToInsert);
        }

        public ITestEnvironment Insert<TBlockToFind, TBlockToInsert>(InsertMode mode,
            Action<TBlockToInsert> setup,
            string markToFind = null,
            string markToInsert = null)
            where TBlockToFind : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var wrappedSetup = new Action<TBlockToInsert, ITestEnvironment>((block, _) => { setup.Invoke(block); });

            return InternalInsert<TBlockToFind, TBlockToInsert>(mode, wrappedSetup, markToFind, markToInsert);
        }

        public ITestEnvironment Insert<TBlockToFind, TBlockToInsert>(InsertMode mode,
            string markToFind = null,
            string markToInsert = null)
            where TBlockToFind : IBlock
        {
            return InternalInsert<TBlockToFind, TBlockToInsert>(mode, null, markToFind, markToInsert);
        }

        public ITestEnvironment Update<TBlock>(string mark, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock
        {
            if (mark == null) throw new ArgumentNullException(nameof(mark));
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            return UpdateInternal(mark, setup);
        }

        public ITestEnvironment Update<TBlock>(Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            return UpdateInternal(null, setup);
        }

        public ITestEnvironment Update<TBlock>(string mark, Action<TBlock> setup)
            where TBlock : IBlock
        {
            if (mark == null) throw new ArgumentNullException(nameof(mark));
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var wrappedSetup = new Action<TBlock, ITestEnvironment>((block, _) => { setup(block); });

            return UpdateInternal(mark, wrappedSetup);
        }

        public ITestEnvironment Update<TBlock>(Action<TBlock> setup)
            where TBlock : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var wrappedSetup = new Action<TBlock, ITestEnvironment>((block, _) => { setup(block); });

            return UpdateInternal(null, wrappedSetup);
        }

        public ITestEnvironment Replace<TBlock>(Action<TBlock, ITestEnvironment> setup,
            string markToFind = null,
            string markToAdd = null) where TBlock : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            return ReplaceInternal(setup, markToFind, markToAdd);
        }

        public ITestEnvironment Replace<TBlock>(Action<TBlock> setup,
            string markToFind = null,
            string markToAdd = null) where TBlock : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var wrappedSetup = new Action<TBlock, ITestEnvironment>((block, _) => { setup(block); });

            return ReplaceInternal(wrappedSetup, markToFind, markToAdd);
        }


        public int IndexOf<TBlock>()
        {
            return IndexOf<TBlock>(null);
        }

        public int IndexOf<TBlock>(string mark)
        {
            var toFindRegistration = new BlockRegistration<TBlock>(mark);
            return registrations.IndexOf(toFindRegistration);
        }


        public ITestEnvironment RemoveAll<TBlock>()
            where TBlock : IBlock
        {
            return RemoveAll<TBlock>(null);
        }

        public ITestEnvironment Remove<TBlock>()
            where TBlock : IBlock
        {
            return RemoveInternal<TBlock>(null);
        }

        public ITestEnvironment Remove<TBlock>(string mark)
            where TBlock : IBlock
        {
            if (mark == null) throw new ArgumentNullException(nameof(mark));

            return RemoveInternal<TBlock>(mark);
        }

        public ITestEnvironment Unwrap<T>() where T : ITestWrapper
        {
            return Unwrap(typeof(T));
        }

        public ITestEnvironment Unwrap(Type wrapper)
        {
            if (wrapper == null) throw new ArgumentNullException(nameof(wrapper));

            if (!typeof(ITestWrapper).IsAssignableFrom(wrapper))
                throw new ArgumentException($"Wrapper must inherit from {nameof(ITestWrapper)}");

            wrappers.Remove(wrapper);

            return this;
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            foreach (var block in blocks) await block.ExecuteAtTheEnd(this).ConfigureAwait(false);

            foreach (var disposableElement in disposables)
                switch (disposableElement)
                {
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                }

            switch (serviceProvider)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }


        internal ITestEnvironment AddInternal<TBlock>(string mark, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock
        {
            ThrowIfAlreadyBuildException();

            var registration = new BlockRegistration<TBlock>(mark, setup);
            ThrowIfAlreadyExists(registration);
            registrations.Add(registration);
            return this;
        }


        internal ITestEnvironment InsertAtInternal<TBlock>(int index, string mark, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock
        {
            ThrowIfAlreadyBuildException();

            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (index > registrations.Count) throw new ArgumentOutOfRangeException(nameof(index));

            var registration = new BlockRegistration<TBlock>(mark, setup);
            ThrowIfAlreadyExists(registration);
            registrations.Insert(index, registration);
            return this;
        }


        internal ITestEnvironment InternalInsert<TBlockToFind, TBlockToInsert>(InsertMode mode,
            Action<TBlockToInsert, ITestEnvironment> setup,
            string markToFind = null,
            string markToInsert = null)
            where TBlockToFind : IBlock
        {
            ThrowIfAlreadyBuildException();

            Find<TBlockToFind>(markToFind, true, out var indexOf);

            var registration = new BlockRegistration<TBlockToInsert>(markToInsert, setup);

            ThrowIfAlreadyExists(registration);

            switch (mode)
            {
                case InsertMode.After:
                    registrations.Insert(indexOf + 1, registration);
                    break;
                case InsertMode.Before:
                    registrations.Insert(indexOf, registration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            return this;
        }


        internal ITestEnvironment UpdateInternal<TBlock>(string mark, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            ThrowIfAlreadyBuildException();

            var found = Find<TBlock>(mark, true, out var __);

            var oldUpdater = new PreviousBlockSetupDelegate<TBlock>(block => { found.Setup?.Invoke(block, this); });

            var updateNew = new Action<TBlock, ITestEnvironment>((block, testEnvironment) =>
            {
                oldUpdater(block);

                setup(block, testEnvironment);
            });

            registrations[registrations.IndexOf(found)] = new BlockRegistration<TBlock>(mark, updateNew);

            return this;
        }


        internal ITestEnvironment ReplaceInternal<TBlock>(Action<TBlock, ITestEnvironment> setup,
            string markToFind, string markToAdd)
            where TBlock : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            ThrowIfAlreadyBuildException();

            RemoveInternal<TBlock>(markToFind);
            return AddInternal(markToAdd, setup);
        }

        public ITestEnvironment RemoveAll<TBlock>(string mark)
            where TBlock : IBlock
        {
            ThrowIfAlreadyBuildException();

            var toRemove = registrations.Where(e => e.BlockType == typeof(TBlock));

            foreach (var block in toRemove.ToArray()) registrations.Remove(block);

            return this;
        }

        public ITestEnvironment RemoveInternal<TBlock>(string mark)
            where TBlock : IBlock
        {
            ThrowIfAlreadyBuildException();

            var found = Find<TBlock>(mark, true, out var __);
            registrations.Remove(found);
            return this;
        }


        private IBlock ResolveBlock(Type typeToResolve, params object[] availableData)
        {
            var ctor = typeToResolve.GetConstructors()
                .Select(e => (ctor: e, count: e.GetParameters().Length))
                .OrderByDescending(e => e.count)
                .Select(e => e.ctor)
                .First();

            var sp = GetServiceProvider(); // never null, if not set then and empty one

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
                    var value = sp.GetService(paramInfo.ParameterType);
                    parameters.Add(value);
                }
                else
                {
                    try
                    {
                        var value = sp.GetRequiredService(paramInfo.ParameterType);
                        parameters.Add(value);
                    }
                    catch (Exception e)
                    {
                        throw new Exception
                        (
                            $"Unable to resolve block: {typeToResolve.FullName} " +
                            $"parameter: {paramInfo.Name} of type {paramInfo.ParameterType.FullName}."
                            , e
                        );
                    }
                }
            }

            return (IBlock)ctor.Invoke(parameters.ToArray());
        }

        private void ThrowIfAlreadyBuildException()
        {
            if (IsBuilt)
                throw new ApplicationException("Test builder is already built and can not be modified.");
        }

        private void ThrowIfAlreadyExists(IBlockRegistration registration)
        {
            if (registrations.Contains(registration))
                throw new ArgumentException($"Block already exists: {registration}.");
        }

        private BlockRegistration<TBlock> Find<TBlock>(string mark, bool throwIfNotFound, out int indexOf)
        {
            var toFindRegistration = new BlockRegistration<TBlock>(mark);

            indexOf = registrations.IndexOf(toFindRegistration);

            if (indexOf == -1)
                if (throwIfNotFound) throw new Exception($"Unable to find: {toFindRegistration}.");
                else return null;

            return (BlockRegistration<TBlock>)registrations[indexOf];
        }

        private sealed class BlockRegistration<TBlock> : IEquatable<BlockRegistration<TBlock>>, IBlockRegistration
        {
            public BlockRegistration(string mark, Action<TBlock, ITestEnvironment> setup = null)
            {
                Mark = mark;
                Setup = setup;
            }

            public Action<TBlock, ITestEnvironment> Setup { get; }
            public Type BlockType => typeof(TBlock);
            public string Mark { get; }

            public bool Equals(BlockRegistration<TBlock> other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;

                return BlockType == other.BlockType && string.Equals(Mark, other.Mark);
            }

            public override bool Equals(object obj)
            {
                return ReferenceEquals(this, obj) || obj is BlockRegistration<TBlock> other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((BlockType != null ? BlockType.GetHashCode() : 0) * 397) ^ (Mark != null ? Mark.GetHashCode() : 0);
                }
            }

            public static bool operator ==(BlockRegistration<TBlock> left, BlockRegistration<TBlock> right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(BlockRegistration<TBlock> left, BlockRegistration<TBlock> right)
            {
                return !Equals(left, right);
            }

            public override string ToString()
            {
                return $"{nameof(BlockType)}: {BlockType}, Mark: {Mark ?? "<null>"}";
            }
        }

        internal interface IBlockRegistration
        {
            Type BlockType { get; }
            string Mark { get; }
        }
    }

    public enum InsertMode
    {
        After,
        Before
    }
}