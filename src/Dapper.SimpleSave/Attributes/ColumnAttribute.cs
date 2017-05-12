using System;

namespace Dapper.SimpleSave.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; private set; }
        public ColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
