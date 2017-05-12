using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Dapper.SimpleSave.Attributes;
using Dapper.SimpleSave.Configuration;

namespace Dapper.SimpleSave.Metadata
{
    public class PropertyMetadataExt: IPropertyMetadataExt
    {
        public PropertyMetadataExt(PropertyInfo prop)
        {
            Prop = prop;
            IsReadOnly = !prop.CanWrite;
            IsPublic = prop.CanRead;
            IsGenericDictionary = typeof(IDictionary<,>).IsAssignableFrom(Prop.PropertyType);
            IsString = typeof(string) == Prop.PropertyType;
            IsEnumerable = !IsString && (IsGenericDictionary
                || typeof(IEnumerable).IsAssignableFrom(Prop.PropertyType)
                || Prop.PropertyType.IsArray);
            IsValueType = Prop.PropertyType.IsValueType;
            IsEnum = IsValueType && Prop.PropertyType.IsEnum;
            IsBool = IsValueType && typeof(bool) == Prop.PropertyType;
            IsDateTime = IsValueType && typeof(DateTime) == Prop.PropertyType;
            IsDateTimeOffset = IsValueType && typeof(DateTimeOffset) == Prop.PropertyType;
            IsNumericType = IsValueType
                            && !(IsEnumerable || IsBool || IsDateTime || IsDateTimeOffset);
            RelationshipType = RelationType.None;
            ColumnName = prop.Name;
            Name = prop.Name;
        }

        public PropertyInfo Prop { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool HasRelationship { get; set; }

        public RelationType RelationshipType { get; set; }

        public MemberRelation Relationship { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsIgnored { get; set; }

        public string Name { get; }

        public bool IsSaveable { get; set; }

        public string ColumnName { get; set; }

        public bool IsSoftDeletable => SoftDelete != null;

        public SoftDeleteData SoftDelete { get; set; }

        public bool IsPublic { get; }

        public bool IsGenericDictionary { get; }

        public bool IsEnumerable { get; }

        public bool IsEnum { get; }

        public bool IsBool { get; }

        public bool IsValueType { get; }

        public bool IsNumericType { get; }

        public bool IsDateTime { get; }

        public bool IsDateTimeOffset { get; }

        public bool IsReferenceType => !IsValueType;

        public bool IsString { get; }
    }
}