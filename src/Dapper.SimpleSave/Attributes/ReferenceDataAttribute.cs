using System;

namespace Dapper.SimpleSave.Attributes
{
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
