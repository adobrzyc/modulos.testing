using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ClassNeverInstantiated.Global

namespace Modulos.Testing
{
    public class TestEnvironment : ITestEnvironment
    {
        private readonly object locker = new object();

        private readonly List<IBlockRegistration> registrations = new List<IBlockRegistration>();
        private readonly List<Type> wrappers = new List<Type>();
        private readonly ConcurrentBag<object> disposables = new ConcurrentBag<object>();
        private readonly ConcurrentBag<IBlock> blocks = new ConcurrentBag<IBlock>();
        private IServiceProvider serviceProvider;
        private bool isBuilt;
        private bool IsBuilt
        {
            get { lock(locker) return isBuilt; }
        }

        public async Task<ITestEnvironment> Build(params object[] data)
        {
            lock (locker)
            {
                if (isBuilt) 
                    throw new ApplicationException("Test builder is already built.");
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
                        ((dynamic) registration).Setup?.Invoke((dynamic) block, (dynamic) this);

                    }
                    catch (RuntimeBinderException e)
                    {
                        throw new AggregateException($"Specified block: {registration.BlockType} is probably defined " +
                                                     "in inaccessible scope e.q.: private. Blocks should be defined as a public classes.",
                            e);
                    }

                    var result = await block.Execute(this);
                    parameters.Add(block);
                    parameters.AddRange(result.PublishedData);
                    blocks.Add(block);

                    switch (result.Action)
                    {
                        case ActionAfterBlock.Continue:
                            continue;
                        case ActionAfterBlock.Break:
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
                {
                    if(param is IAsyncDisposable || param is IDisposable)
                        disposables.Add(param);
                }
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

        /// <summary>
        /// Creates test class inherited from <see cref="ITest"/> and build test environment if not already built. 
        /// </summary>
        /// <typeparam name="TTest">Type of the test class.</typeparam>
        /// <param name="optionsModifier">Options used to create test.</param>
        /// <returns>Instance of the test class.</returns>
        public virtual async Task<TTest> CreateTest<TTest>(Action<TestOptions> optionsModifier = null)
            where TTest : ITest
        {
            if(!IsBuilt)
                throw new ApplicationException($"Before call {nameof(CreateTest)}, {nameof(Build)} method must be called first.");

            var options = new TestOptions();
            foreach (var wrapper in wrappers)
            {
                options.Wrap(wrapper);
            }
            optionsModifier?.Invoke(options);

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

                if (param.ParameterType == typeof(TestOptions))
                {
                    parameters.Add(options);
                    continue;
                }

                if ((param.Attributes & ParameterAttributes.Optional) != 0)
                {
                    parameters.Add(sp.GetService(param.ParameterType));
                }
                else
                {
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
            }

            var test =  (TTest)Activator.CreateInstance(typeof(TTest), parameters.ToArray());

            await test.BeginWrappers();

            return test;
        }

        public Task<Test> CreateTest(Action<TestOptions> optionsModifier = null)
        {
            return CreateTest<Test>(optionsModifier);
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

        protected virtual async ValueTask DisposeAsyncCore()
        {
            foreach (var block in blocks)
            {
                await block.ExecuteAtTheEnd(this).ConfigureAwait(false);
            }

            foreach (var disposableElement in disposables)
            {
                switch (disposableElement)
                {
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                }
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

 

        public void Wrap<T>() where T : ITestWrapper
        {
            Wrap(typeof(T));
        }

        public void Wrap(Type wrapper)
        {
            if (wrapper == null) throw new ArgumentNullException(nameof(wrapper));

            if(!typeof(ITestWrapper).IsAssignableFrom(wrapper))
                throw new ArgumentException($"Wrapper must inherit from {nameof(ITestWrapper)}");

            wrappers.Add(wrapper);
        }


        public ITestEnvironment Add<TBlock>(string mark, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock
        {

            ThrowIfAlreadyBuildException();

            var registration = new BlockRegistration<TBlock>(mark, setup);
            ThrowIfAlreadyExists(registration);
            registrations.Add(registration);
            return this;
        }

        public ITestEnvironment Add<TBlock>(Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock
        {
            return Add(null, setup);
        }

        public ITestEnvironment Add<TBlock>(string mark) 
            where TBlock : IBlock
        {
            return Add<TBlock>(mark, null);
        }

        public ITestEnvironment Add<TBlock>() where TBlock : IBlock
        {
            return Add<TBlock>(null, null);
        }



        public ITestEnvironment Update<TBlock>(Action<TBlock, ITestEnvironment, PreviousBlockSetupDelegate<TBlock>> setup)
            where TBlock : IBlock
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            return Update(null, setup);
        }

        public ITestEnvironment Update<TBlock>(string mark, Action<TBlock, ITestEnvironment, PreviousBlockSetupDelegate<TBlock>> setup)
            where TBlock : IBlock
        {
            ThrowIfAlreadyBuildException();

            var found = Find<TBlock>(mark, true, out var __);
            
            var oldUpdater = new PreviousBlockSetupDelegate<TBlock>(block => { found.Setup?.Invoke(block, this); });

            var updateNew = new Action<TBlock, ITestEnvironment>((block, builder) =>
            {
                setup(block, builder, oldUpdater);
            });

            registrations[registrations.IndexOf(found)] = new BlockRegistration<TBlock>(mark, updateNew);

            return this;
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

        public ITestEnvironment RemoveAll<TBlock>(string mark)
            where TBlock : IBlock
        {
            ThrowIfAlreadyBuildException();


            var toRemove = registrations.Where(e => e.BlockType == typeof(TBlock) && e.Mark == mark);

            foreach (var block in toRemove)
            {
                registrations.Remove(block);
            }

            return this;
        }


        public ITestEnvironment Remove<TBlock>()
            where TBlock : IBlock
        {
            return Remove<TBlock>(null);
        }

        public ITestEnvironment Remove<TBlock>(string mark)
            where TBlock : IBlock
        {
            ThrowIfAlreadyBuildException();

            var found = Find<TBlock>(mark, true, out var __);
            registrations.Remove(found);
            return this;
        }


        public ITestEnvironment Insert<TBlockToFind, TBlockInsert>(InsertType insertType, 
            string markToFind = null, 
            string markToInsert = null, 
            Action<TBlockInsert, ITestEnvironment> setup = null)

            where TBlockToFind : IBlock
        {
            ThrowIfAlreadyBuildException();


            Find<TBlockToFind>(markToFind, true, out var indexOf);

            var registration = new BlockRegistration<TBlockInsert>(markToInsert, setup);
 
            ThrowIfAlreadyExists(registration);

            switch (insertType)
            {
                case InsertType.After:
                    registrations.Insert(indexOf + 1, registration);
                    break;
                case InsertType.Before:
                    if (indexOf == 0)
                        indexOf = 1;
                    registrations.Insert(indexOf, registration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(insertType), insertType, null);
            }

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
            if(IsBuilt)
                throw new ApplicationException("Test builder is already built and can not be modified.");
        }

        private sealed class BlockRegistration<TBlock> : IEquatable<BlockRegistration<TBlock>>, IBlockRegistration
        {
            public Type BlockType => typeof(TBlock);
            public string Mark { get; }
            public Action<TBlock, ITestEnvironment> Setup { get; }

            public BlockRegistration(string mark, Action<TBlock, ITestEnvironment> setup = null)
            {
                Mark = mark;
                Setup = setup;
            }

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


        public enum InsertType
        {
            After, Before
        }

        private interface IBlockRegistration
        {
            Type BlockType { get; }
            string Mark { get; }
        }

    }
}