// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace Modulos.Testing
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the test environment.
    /// </summary>
    /// <remarsk>
    /// Lifetime of this object should be managed by test framework like a xUnit, NUnit.
    /// </remarsk>
    public interface ITestEnvironment : IAsyncDisposable
    {
        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="updateOptions">Allows editing options.</param>
        /// <typeparam name="TTest">The test type.</typeparam>
        /// <typeparam name="TOptions">The options type.</typeparam>
        /// <returns>The instance of the built test.</returns>
        Task<TTest> CreateTest<TTest, TOptions>(Action<TOptions> updateOptions = null)
            where TTest : ITest
            where TOptions : ITestOptions, new();

        /// <summary>
        /// Creates the <see cref="Test" /> instance.
        /// </summary>
        /// <param name="updateOptions">Allows editing options.</param>
        /// <returns>The instance of the built test.</returns>
        Task<Test> CreateTest(Action<TestOptions> updateOptions = null);
       
        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="updateOptions">Allows editing options.</param>
        /// <typeparam name="TTest">The test type.</typeparam>
        /// <returns>The instance of the built test.</returns>
        Task<TTest> CreateTest<TTest>(Action<TestOptions> updateOptions = null)
            where TTest : ITest;
        
        /// <summary>
        /// Builds the test environment based on previously defined blocks.
        /// </summary>
        /// <param name="data">Additional data that can be used by blocks.</param>
        /// <returns>The instance of the test environment.</returns>
        Task<ITestEnvironment> Build(params object[] data);

        /// <summary>
        /// Returns <see cref="IServiceProvider" /> defined by <see cref="SetServiceProvider" /> method.
        /// If there is no <see cref="SetServiceProvider" /> call before, an empty <see cref="IServiceProvider" />
        /// will be returned.
        /// </summary>
        /// <returns>
        /// <see cref="IServiceProvider" /> instance set by <see cref="SetServiceProvider" /> or an empty
        /// instance if there is no <see cref="SetServiceProvider" /> call before.
        /// </returns>
        IServiceProvider GetServiceProvider();

        /// <summary>
        /// Sets service provider for the test environment.
        /// </summary>
        /// <param name="provider">Provider instance to set.</param>
        void SetServiceProvider(IServiceProvider provider);

        /// <summary>
        /// Wraps test with the specified wrapper.
        /// </summary>
        /// <typeparam name="TWrapper">The type of the wrapper.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Wrap<TWrapper>() where TWrapper : ITestWrapper;

        /// <summary>
        /// Wraps test with the specified wrapper. Passed type must inherit from <see cref="ITestWrapper" />.
        /// </summary>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Wrap(Type wrapperType);


        /// <summary>
        /// Adds disposable element that will be disposed together with the test environment.
        /// </summary>
        /// <param name="disposable">Disposable element.</param>
        void AddDisposableElement(IDisposable disposable);


        /// <summary>
        /// Adds block to the environment definition.
        /// </summary>
        /// <typeparam name="TBlock">The block type.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Add<TBlock>() where TBlock : IBlock;

        /// <summary>
        /// Adds block to the environment definition.
        /// </summary>
        /// <param name="mark">The mark of the block.</param>
        /// <typeparam name="TBlock">The block type.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Add<TBlock>(string mark) where TBlock : IBlock;

        /// <summary>
        /// Adds block to the environment definition.
        /// </summary>
        /// <param name="setup">The setup action.</param>
        /// <typeparam name="TBlock">The block type.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Add<TBlock>(Action<TBlock> setup) where TBlock : IBlock;
     
        /// <summary>
        /// Adds block to the environment definition.
        /// </summary>
        /// <param name="setup">The setup action.</param>
        /// <typeparam name="TBlock">The block type.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Add<TBlock>(Action<TBlock, ITestEnvironment> setup) where TBlock : IBlock;

        /// <summary>
        /// Adds block to the environment definition.
        /// </summary>
        /// <param name="mark">The mark of the block.</param>
        /// <param name="setup">The setup action.</param>
        /// <typeparam name="TBlock">The block type.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Add<TBlock>(string mark, Action<TBlock> setup) where TBlock : IBlock;

        /// <summary>
        /// Adds block to the environment definition.
        /// </summary>
        /// <param name="mark">The mark of the block.</param>
        /// <param name="setup">The setup action.</param>
        /// <typeparam name="TBlock">The block type.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Add<TBlock>(string mark, Action<TBlock, ITestEnvironment> setup) where TBlock : IBlock;


        /// <summary>
        /// Inserts block according to specified parameters.
        /// </summary>
        /// <param name="mode">The insert mode.</param>
        /// <param name="setup">The setup action.</param>
        /// <param name="markToFind">Mark to find.</param>
        /// <param name="markToInsert">Mark to insert.</param>
        /// <typeparam name="TBlockToFind">Block to find.</typeparam>
        /// <typeparam name="TBlockInsert">Block to insert.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Insert<TBlockToFind, TBlockInsert>(InsertMode mode,
            Action<TBlockInsert, ITestEnvironment> setup,
            string markToFind = null,
            string markToInsert = null) where TBlockToFind : IBlock;

        /// <summary>
        /// Inserts block according to specified parameters.
        /// </summary>
        /// <param name="mode">The insert mode.</param>
        /// <param name="setup">The setup action.</param>
        /// <param name="markToFind">Mark to find.</param>
        /// <param name="markToInsert">Mark to insert.</param>
        /// <typeparam name="TBlockToFind">Block to find.</typeparam>
        /// <typeparam name="TBlockInsert">Block to insert.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Insert<TBlockToFind, TBlockInsert>(InsertMode mode,
            Action<TBlockInsert> setup,
            string markToFind = null,
            string markToInsert = null) where TBlockToFind : IBlock;

        /// <summary>
        /// Inserts block according to specified parameters.
        /// </summary>
        /// <param name="mode">The insert mode.</param>
        /// <param name="markToFind">Mark to find.</param>
        /// <param name="markToInsert">Mark to insert.</param>
        /// <typeparam name="TBlockToFind">Block to find.</typeparam>
        /// <typeparam name="TBlockInsert">Block to insert.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Insert<TBlockToFind, TBlockInsert>(InsertMode mode,
            string markToFind = null,
            string markToInsert = null) where TBlockToFind : IBlock;

        /// <summary>
        /// Inserts block according to specified parameters.
        /// </summary>
        /// <param name="index">Index to put block on it.</param>
        /// <param name="mark">The mark of the block.</param>
        /// <param name="setup">The setup action.</param>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment InsertAt<TBlock>(int index, string mark, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock;
        
        /// <summary>
        /// Inserts block according to specified parameters.
        /// </summary>
        /// <param name="index">Index to put block on it.</param>
        /// <param name="mark">The mark of the block.</param>
        /// <param name="setup">The setup action.</param>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment InsertAt<TBlock>(int index, string mark, Action<TBlock> setup)
            where TBlock : IBlock;
    
        /// <summary>
        /// Inserts block according to specified parameters.
        /// </summary>
        /// <param name="index">Index to put block on it.</param>
        /// <param name="setup">The setup action.</param>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment InsertAt<TBlock>(int index, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock;

        /// <summary>
        /// Inserts block according to specified parameters.
        /// </summary>
        /// <param name="index">Index to put block on it.</param>
        /// <param name="setup">The setup action.</param>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment InsertAt<TBlock>(int index, Action<TBlock> setup)
            where TBlock : IBlock;

        /// <summary>
        /// Inserts block according to specified parameters.
        /// </summary>
        /// <param name="index">Index to put block on it.</param>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment InsertAt<TBlock>(int index)
            where TBlock : IBlock;

        /// <summary>
        /// Inserts block according to specified parameters.
        /// </summary>
        /// <param name="index">Index to put block on it.</param>
        /// <param name="mark">The mark of the block.</param>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment InsertAt<TBlock>(int index, string mark)
            where TBlock : IBlock;


        /// <summary>
        /// Defines the update action to perform on specified block.
        /// </summary>
        /// <param name="mark">Mark of the block.</param>
        /// <param name="setup">Action to perform on block.</param>
        /// <typeparam name="TBlock">Type of block to find.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Update<TBlock>(string mark, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock;

        /// <summary>
        /// Defines the update action to perform on specified block.
        /// </summary>
        /// <param name="setup">Action to perform on block.</param>
        /// <typeparam name="TBlock">Type of block to find.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Update<TBlock>(Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock;

        /// <summary>
        /// Defines the update action to perform on specified block.
        /// </summary>
        /// <param name="mark">Mark of the block.</param>
        /// <param name="setup">Action to perform on block.</param>
        /// <typeparam name="TBlock">Type of block to find.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Update<TBlock>(string mark, Action<TBlock> setup)
            where TBlock : IBlock;

        /// <summary>
        /// Defines the update action to perform on specified block.
        /// </summary>
        /// <param name="setup">Action to perform on block.</param>
        /// <typeparam name="TBlock">Type of block to find.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Update<TBlock>(Action<TBlock> setup)
            where TBlock : IBlock;


        /// <summary>
        /// Replaces specified block with new one.
        /// </summary>
        /// <param name="setup">Action to perform on new block.</param>
        /// <param name="markToFind">Mark to find.</param>
        /// <param name="markToAdd">Mark to add.</param>
        /// <typeparam name="TBlock">Type of block to find.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Replace<TBlock>(Action<TBlock, ITestEnvironment> setup,
            string markToFind = null, string markToAdd = null) where TBlock : IBlock;

        /// <summary>
        /// Replaces specified block with new one.
        /// </summary>
        /// <param name="setup">Action to perform on new block.</param>
        /// <param name="markToFind">Mark to find.</param>
        /// <param name="markToAdd">Mark to add.</param>
        /// <typeparam name="TBlock">Type of block to find.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Replace<TBlock>(Action<TBlock> setup,
            string markToFind = null, string markToAdd = null) where TBlock : IBlock;


        /// <summary>
        /// Returns index of the specified block.
        /// </summary>
        /// <param name="mark">Mark of block to find.</param>
        /// <typeparam name="TBlock">Type of block to find.</typeparam>
        /// <returns>The test environment instance.</returns>
        int IndexOf<TBlock>(string mark);
        
        /// <summary>
        /// Returns index of the specified block.
        /// </summary>
        /// <typeparam name="TBlock">Type of block to find.</typeparam>
        /// <returns>The test environment instance.</returns>
        int IndexOf<TBlock>();


        /// <summary>
        /// Removes all block of specified type.
        /// </summary>
        /// <typeparam name="TBlock">Type of block to remove.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment RemoveAll<TBlock>()
            where TBlock : IBlock;

        /// <summary>
        /// Removes all block of specified type.
        /// </summary>
        /// <typeparam name="TBlock">Type of block to remove.</typeparam>
        /// <param name="mark">Mark of the block to remove.</param>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Remove<TBlock>(string mark)
            where TBlock : IBlock;
       
        /// <summary>
        /// Removes all block of specified type.
        /// </summary>
        /// <typeparam name="TBlock">Type of block to remove.</typeparam>
        /// <returns>The test environment instance.</returns>
        ITestEnvironment Remove<TBlock>()
            where TBlock : IBlock;


    }
}