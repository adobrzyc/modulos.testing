using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Modulos.Testing
{
    /// <summary>
    /// Lifetime of this object should be managed by test framework like a xUnit, NUnit.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface ITestEnvironment : IAsyncDisposable
    {
        Task<TTest> CreateTest<TTest>(Action<TestOptions> optionsModifier = null)
            where TTest : ITest;

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

        void AddDisposableElement(IDisposable disposable);

        ITestEnvironment Add<TBlock>() where TBlock : IBlock;

        ITestEnvironment Add<TBlock>(string mark) where TBlock : IBlock;

        ITestEnvironment Add<TBlock>(Action<TBlock, ITestEnvironment> setup) where TBlock : IBlock;

        ITestEnvironment Add<TBlock>(string mark, Action<TBlock, ITestEnvironment> setup) where TBlock : IBlock;


        ITestEnvironment Update<TBlock>(string mark, Action<TBlock, ITestEnvironment, PreviousBlockSetupDelegate<TBlock>> setup)
            where TBlock : IBlock;

        ITestEnvironment Update<TBlock>(Action<TBlock, ITestEnvironment, PreviousBlockSetupDelegate<TBlock>> setup)
            where TBlock : IBlock;

   

        int IndexOf<TBlock>(string mark);

        int IndexOf<TBlock>();


        ITestEnvironment RemoveAll<TBlock>(string mark)
            where TBlock : IBlock;

        ITestEnvironment RemoveAll<TBlock>()
            where TBlock : IBlock;


        ITestEnvironment Remove<TBlock>(string mark)
            where TBlock : IBlock;

        ITestEnvironment Remove<TBlock>()
            where TBlock : IBlock;

        ITestEnvironment Insert<TBlockToFind, TBlockInsert>(TestEnvironment.InsertType insertType, 
            string markToFind = null, 
            string markToInsert = null, Action<TBlockInsert, ITestEnvironment> setup = null) where TBlockToFind : IBlock;


        Task<Test> CreateTest(Action<TestOptions> optionsModifier = null);
        void Wrap<T>() where T : ITestWrapper;
        void Wrap(Type wrapper);
        Task<ITestEnvironment> Build(params object[] data);
    }
}