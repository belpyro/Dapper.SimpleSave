using System.Reflection;

namespace Dapper.SimpleSave.Metadata
{
    public interface IPropertyMetadata
    {
        PropertyInfo Prop { get; set; }
        bool IsPrimaryKey { get; }
        bool IsManyToManyRelationship { get; }
        bool IsOneToManyRelationship { get; }
        bool IsManyToOneRelationship { get; }
        bool IsOneToOneRelationship { get; }
        bool IsReadOnly { get; }
        bool IsPublic { get; }
        bool IsSaveable { get; }
        bool IsGenericDictionary { get; }
        bool IsEnumerable { get; }
        bool IsEnum { get; }
        bool IsBool { get; }
        bool IsValueType { get; }
        bool IsNumericType { get; }
        bool IsDateTime { get; }
        bool IsDateTimeOffset { get; }
        bool IsReferenceType { get; }
        bool IsString { get; }
        string ColumnName { get; }
        bool IsSoftDeletable { get; }
    }
}