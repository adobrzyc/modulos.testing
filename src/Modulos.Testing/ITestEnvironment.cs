using System;
using System.Threading.Tasks;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace Modulos.Testing
{
    /// <summary>
    /// Lifetime of this object should be managed by test framework like a xUnit, NUnit.
    /// </summary>
    public interface ITestEnvironment : IAsyncDisposable
    {
        Task<TTest> CreateTest<TTest>(Action<TestOptions> optionsModifier = null)
            where TTest : ITest;

        Task<Test> CreateTest(Action<TestOptions> optionsModifier = null);
       
        Task<ITestEnvironment> Build(params object[] data);

        /// <summary>
        /// Returns <see cref="IServiceProvider"/> defined by <see cref="SetServiceProvider"/>.
        /// If there is no <see cref="SetServiceProvider"/> call before, an empty <see cref="IServiceProvider"/>
        /// will be returned. 
        /// </summary>
        /// <returns>
        /// <see cref="IServiceProvider"/> instance set by <see cref="SetServiceProvider"/> or an empty
        /// instance if there is no <see cref="SetServiceProvider"/> call before. 
        /// </returns>
        IServiceProvider GetServiceProvider();

        void SetServiceProvider(IServiceProvider provider);


        ITestEnvironment Wrap<T>() where T : ITestWrapper;
       
        ITestEnvironment Wrap(Type wrapper);
        

        void AddDisposableElement(IDisposable disposable);

        
        ITestEnvironment Add<TBlock>() where TBlock : IBlock;

        ITestEnvironment Add<TBlock>(string mark) where TBlock : IBlock;

        ITestEnvironment Add<TBlock>(Action<TBlock> setup) where TBlock : IBlock;

        ITestEnvironment Add<TBlock>(Action<TBlock, ITestEnvironment> setup) where TBlock : IBlock;

        ITestEnvironment Add<TBlock>(string mark, Action<TBlock> setup) where TBlock : IBlock;

        ITestEnvironment Add<TBlock>(string mark, Action<TBlock, ITestEnvironment> setup) where TBlock : IBlock;


        
        ITestEnvironment Insert<TBlockToFind, TBlockInsert>(InsertMode mode,  
            Action<TBlockInsert, ITestEnvironment> setup,
            string markToFind = null,
            string markToInsert = null) where TBlockToFind : IBlock;

        ITestEnvironment Insert<TBlockToFind, TBlockInsert>(InsertMode mode, 
            Action<TBlockInsert> setup,
            string markToFind = null, 
            string markToInsert = null) where TBlockToFind : IBlock;
        
        ITestEnvironment Insert<TBlockToFind, TBlockInsert>(InsertMode mode,
            string markToFind = null,
            string markToInsert = null)  where TBlockToFind : IBlock;


        ITestEnvironment InsertAt<TBlock>(int index, string mark, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock;
        ITestEnvironment InsertAt<TBlock>(int index, string mark, Action<TBlock> setup)
            where TBlock : IBlock;
        ITestEnvironment InsertAt<TBlock>(int index, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock;
        ITestEnvironment InsertAt<TBlock>(int index, Action<TBlock> setup)
            where TBlock : IBlock;
        ITestEnvironment InsertAt<TBlock>(int index)
            where TBlock : IBlock;
        ITestEnvironment InsertAt<TBlock>(int index, string mark)
            where TBlock : IBlock;


        ITestEnvironment Update<TBlock>(string mark, Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock;

        ITestEnvironment Update<TBlock>(Action<TBlock, ITestEnvironment> setup)
            where TBlock : IBlock;

        ITestEnvironment Update<TBlock>(string mark, Action<TBlock> setup)
            where TBlock : IBlock;

        ITestEnvironment Update<TBlock>(Action<TBlock> setup)
            where TBlock : IBlock;


        ITestEnvironment Replace<TBlock>(Action<TBlock, ITestEnvironment> setup,
            string markToFind = null, string markToAdd = null) where TBlock : IBlock;

        ITestEnvironment Replace<TBlock>(Action<TBlock> setup,
            string markToFind = null, string markToAdd = null) where TBlock : IBlock;



        int IndexOf<TBlock>(string mark);

        int IndexOf<TBlock>();


        ITestEnvironment RemoveAll<TBlock>()
            where TBlock : IBlock;


        ITestEnvironment Remove<TBlock>(string mark)
            where TBlock : IBlock;

        ITestEnvironment Remove<TBlock>()
            where TBlock : IBlock;


   
    }
}