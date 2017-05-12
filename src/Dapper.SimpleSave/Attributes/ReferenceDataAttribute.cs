using System;

namespace Dapper.SimpleSave.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Enum)]
    public class ReferenceDataAttribute : Attribute
    {
        public ReferenceDataAttribute()
        {
        }

        public ReferenceDataAttribute(bool hasUpdateableForeignKeys)
        {
            HasUpdateableForeignKeys = hasUpdateableForeignKeys;
        }

        public bool HasUpdateableForeignKeys { get; private set; }
    }
}
