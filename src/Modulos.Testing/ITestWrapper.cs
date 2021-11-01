namespace Modulos.Testing
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines test wrapper.
    /// </summary>
    public interface ITestWrapper
    {
        /// <summary>
        /// Executed by <see cref="ITest.BeginWrappers"/> method. 
        /// </summary>
        Task Begin();
        
        /// <summary>
        /// Executed by <see cref="ITest.FinishWrappers"/> method. 
        /// </summary>
        Task Finish();
    }
}