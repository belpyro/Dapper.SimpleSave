using System.Reflection;
using Dapper.SimpleSave.Attributes;
using Dapper.SimpleSave.Configuration;

namespace Dapper.SimpleSave.Metadata
{
    public interface IPropertyMetadataExt
    {
        PropertyInfo Prop { get; }

        bool IsPrimaryKey { get; set; }

        bool HasRelationship { get; set; }

        RelationType RelationshipType { get; set; }

        MemberRelation Relationship { get; set; }

        bool IsReadOnly { get; set; }

        bool IsIgnored { get; set; }

        bool IsPublic { get; }

        string Name { get; }

        bool IsSaveable { get; set; }

        string ColumnName { get; set; }
        SoftDeleteData SoftDelete { get; set; }

        bool IsSoftDeletable { get; }

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
    }
}