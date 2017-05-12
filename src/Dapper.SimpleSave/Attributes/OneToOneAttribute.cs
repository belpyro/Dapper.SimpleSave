using System;

namespace Dapper.SimpleSave.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OneToOneAttribute : Attribute
    {
        public OneToOneAttribute()
        {
        }

        public OneToOneAttribute(string childForeignKeyColumn)
        {
            ChildForeignKeyColumn = childForeignKeyColumn;
        }

        public string ChildForeignKeyColumn { get; private set; }
    }
}
