using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// ReSharper disable UnusedMember.Global

namespace Modulos.Testing.EF
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class Extensions
    {
        /// <summary>
        /// Executes assertion after cleanup of all local data. 
        /// </summary>
        /// <typeparam name="TContext">Database context type.</typeparam>
        /// <param name="test">Instance of <see cref="ITest"/>.</param>
        /// <param name="assertion">Assertion logic.</param>
        /// <returns>Task instance.</returns>
        public static async Task AssertDb<TContext>(this ITest test, Func<TContext, Task> assertion) 
            where TContext : DbContext
        {
            var db = test.Resolve<TContext>();
            db.ClearLocals();
            await assertion(db);
        }

        /// <summary>
        /// Cleans all local data for specified context.
        /// </summary>
        /// <param name="context">Context to clean.</param>
        public static void ClearLocals(this DbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            foreach (var keyValuePair in context.GetDbSetsFromContext())
            {
                keyValuePair.Value.Local.Clear();
            }
        }

        /// <summary>
        /// Returns collection of <see cref="DbSet{TEntity}"/> for particular <param name="context"/>.
        /// </summary>
        /// <param name="context">Context to explore.</param>
        /// <returns>Collection of <see cref="DbSet{TEntity}"/>.</returns>
        internal static Dictionary<Type, dynamic> GetDbSetsFromContext(this DbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var dbsets = context.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic)
                .Where(e => e.PropertyType.IsGenericType && e.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(e => new
                {
                    type = e.PropertyType.GenericTypeArguments[0],
                    dbset = (dynamic)e.GetValue(context)
                }).ToDictionary(e => e.type, e => e.dbset);
            return dbsets;
        }
    }
}