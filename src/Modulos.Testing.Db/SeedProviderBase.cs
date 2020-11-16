using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Modulos.Testing
{
    public abstract class SeedProviderBase : ISeedProvider
    {
        #region Fields

        private readonly object locker = new object();
        private  readonly List<object> excludedObjects = new List<object>();
        private readonly List<ModelDefinition> models = new List<ModelDefinition>();

        public IEnumerable<object> ExcludedObjects
        {
            get
            {
                lock (locker) return excludedObjects;
            }
        }
       
        public IEnumerable<ModelDefinition> Model
        {
            get
            {
                lock (locker) return models.AsReadOnly();//.Select(e => e.ClassType);
            }
        }

        #endregion

        public abstract object GetDb();

        public ISeedProvider Add<TModel>() where TModel : class
        {
            return AddInternal(typeof(TModel));
        }

        public ISeedProvider ExcludeObjects(params object[] objects)
        {
            lock (locker)
            {
                excludedObjects.AddRange(objects);
            }

            return this;
        }

        public abstract Task DropAndCreateDb();

        public abstract Task Seed();
       
        protected IEnumerable<EntityWithOperation> GetEntitiesWithOperationFromClass(Type source)
        {
            var excluded = ExcludedObjects.ToArray();

            var properties = source.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(e => e.GetCustomAttribute<InlineDataAttribute>() != null)
                .ToArray();

            ////todo: does it make sense ?
            //var method = source.GetMethod("Update", BindingFlags.Public | BindingFlags.Static);
            //method?.Invoke(null, null);

            var entities = new List<EntityWithOperation>();

            var enumerables = properties.Where(e => typeof(IEnumerable).IsAssignableFrom(e.PropertyType))
                .Select(e => new
                {
                    data = e.GetValue(null) as IEnumerable,
                    e.GetCustomAttribute<InlineDataAttribute>().OperationKind
                });

            foreach (var enumerable in enumerables)
            {
                var enumerator = enumerable.data.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    entities.Add(new EntityWithOperation(enumerator.Current, enumerable.OperationKind));
                }
            }

            entities.AddRange
            (
                properties.Where(e => !typeof(IEnumerable).IsAssignableFrom(e.PropertyType))
                    .Select(e => new EntityWithOperation
                    (
                        e.GetValue(null),
                        e.GetCustomAttribute<InlineDataAttribute>().OperationKind
                    ))
            );

            return entities.Where(e => !excluded.Contains(e.Entity)).ToArray();
        }

        private ISeedProvider AddInternal(Type modelType)
        {
            var attr = modelType.GetCustomAttribute<ModelDefinitionAttribute>();

            if (attr == null)
                throw new ArgumentException($"{modelType.Name} is not marked with {nameof(ModelDefinitionAttribute)}.");

            var model = new ModelDefinition(modelType);

            if (models.Contains(model))
                throw new Exception($"Model: {model} already exists.");

            models.Add(model);

            var nested = modelType.GetNestedTypes()
                .Where(e => e.GetCustomAttribute<ModelDefinitionAttribute>() != null);

            foreach (var nestedModel in nested)
            {
                AddInternal(nestedModel);
            }

            var included = modelType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(e => e.GetCustomAttribute<IncludeModelAttribute>() != null)
                .SelectMany(e =>
                {
                    var value = e.GetValue(null);
                    if (value.GetType() == typeof(Type))
                        return new[] { (Type)value };
                    return value as IEnumerable<Type>;
                }).ToArray();

            if (included.Any(e => e == null))
                throw new ArgumentException($"Properties marked with {nameof(IncludeModelAttribute)} " +
                                            $"must return Type or IEnumerable<Type>. ");

            foreach (var toInclude in included)
            {
                AddInternal(toInclude);
            }
            return this;
        }


        #region Nested types

        protected class EntityWithOperation
        {
            public object Entity { get; }
            public OperationKind Operation { get; }

            public EntityWithOperation(object entity, OperationKind operation)
            {
                Entity = entity;
                Operation = operation;
            }
        }

        #endregion

    }
}