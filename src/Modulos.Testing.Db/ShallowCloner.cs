using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Modulos.Testing
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ShallowCloner 
    {
        private readonly Func<object, object> _cloneFunc;

        public ShallowCloner()
        {
            var methodInfo = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            var p = Expression.Parameter(typeof(object));
            
            // ReSharper disable once AssignNullToNotNullAttribute
            var mce = Expression.Call(p, methodInfo);
            _cloneFunc = Expression.Lambda<Func<object, object>>(mce, p).Compile();
        }

        public object Clone(object entity)
        {
            var clone = _cloneFunc(entity);
            var cloneProperties = clone.GetType().GetProperties();

            var refProperties = cloneProperties
                .Where(p => !p.PropertyType.IsValueType && p.PropertyType != typeof(string))
                .ToList();

            foreach (var property in refProperties)
            {
                var propValue = property.GetValue(clone);
                if (propValue != null)
                {
                    if (propValue is byte[] byteArray)
                    {
                        property.SetValue(clone, byteArray.Clone());
                    }
                    else
                    {
                        property.SetValue(clone, null);
                    }
                }
            }

            return clone;
        }
    }
}