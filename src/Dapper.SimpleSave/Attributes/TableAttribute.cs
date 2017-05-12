using System;

namespace Dapper.SimpleSave.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
    public class TableAttribute : Attribute
    {
        public string SchemaQualifiedTableName { get; private set; }

        public TableAttribute()
        {
        }

        public TableAttribute(string schemaQualifiedTableName)
        {
            SchemaQualifiedTableName = schemaQualifiedTableName;
        }
    }
}
