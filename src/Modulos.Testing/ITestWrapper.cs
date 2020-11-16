using System.Threading.Tasks;

namespace Modulos.Testing
{
    public interface ITestWrapper
    {
        Task Begin();
        Task Finish();
    }
}