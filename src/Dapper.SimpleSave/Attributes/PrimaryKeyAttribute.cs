using System;

namespace Dapper.SimpleSave.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        public PrimaryKeyAttribute() : this(false)
        {
        }

        public PrimaryKeyAttribute(bool isUserAssigned)
        {
            IsUserAssigned = isUserAssigned;
        }

        public bool IsUserAssigned { get; private set; }
    }
}
