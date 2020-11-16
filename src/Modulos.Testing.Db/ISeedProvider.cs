using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modulos.Testing
{
    public interface ISeedProvider
    {
        IEnumerable<object> ExcludedObjects { get; }
        IEnumerable<ModelDefinition> Model { get; }
        ISeedProvider Add<TModel>() where TModel : class;
        ISeedProvider ExcludeObjects(params object[] objects);
        
        /// <summary>
        /// Returns object to maintain database, eq. DbContext from EntityFramework.
        /// </summary>
        /// <returns>Object to maintain database.</returns>
        object GetDb();
        Task DropAndCreateDb();
        Task Seed();
    }
}