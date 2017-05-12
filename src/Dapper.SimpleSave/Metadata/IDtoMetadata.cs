using System;
using System.Collections.Generic;

namespace Dapper.SimpleSave.Metadata
{
    public interface IDtoMetadata
    {
        Type DtoType { get; }
        string TableName { get; }
        bool IsReferenceData { get; }
        bool HasUpdateableForeignKeys { get; }
        PropertyMetadata PrimaryKey { get; }
        IEnumerable<PropertyMetadata> WriteableProperties { get; }
        IEnumerable<PropertyMetadata> AllProperties { get; }
        PropertyMetadata this[string propertyColumnNameCaseInsensitive] { get; }
    }
}