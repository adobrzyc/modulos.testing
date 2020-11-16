using System;

namespace Modulos.Testing
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class InlineDataAttribute : Attribute
    {
        public OperationKind OperationKind { get; }

        public InlineDataAttribute(OperationKind operationKind = OperationKind.Default)
        {
            OperationKind = operationKind;
        }
    }
}