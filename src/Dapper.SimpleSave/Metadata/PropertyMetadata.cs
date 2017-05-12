using System;
using System.Reflection;
using Dapper.SimpleSave.Attributes;
using Dapper.SimpleSave.Impl;
using Fasterflect;

namespace Dapper.SimpleSave.Metadata
{
    public class PropertyMetadata : BaseMetadata
    {
        public PropertyInfo Prop { get; set; }

        public PropertyMetadata(PropertyInfo prop) : base(prop)
        {
            Prop = prop;

            InitProps();
            InitDictionary();
            InitEnumerable();
            InitValueType();
            InitEnum();
            InitBool();
            InitDateTime();
            InitDateTimeOffset();
            InitNumericType();
            InitString();
            InitReferenceType();
        }

        private void InitProps()
        {
            IsPrimaryKey = Prop.HasAttribute<PrimaryKeyAttribute>();
            IsManyToManyRelationship = Prop.HasAttribute<ManyToManyAttribute>();
            IsOneToManyRelationship = Prop.HasAttribute<OneToManyAttribute>();
            IsManyToOneRelationship = Prop.HasAttribute<ManyToOneAttribute>();
            IsOneToOneRelationship = Prop.HasAttribute<OneToOneAttribute>();
            IsSoftDeletable = Prop.HasAttribute<SoftDeleteColumnAttribute>();
            IsReadOnly = Prop.HasAttribute<SimpleSaveIgnoreAttribute>() || !Prop.CanWrite;
            IsPublic = Prop.IsReadable();
            ColumnName = Prop.HasAttribute<ColumnAttribute>() ? Prop.Attribute<ColumnAttribute>().Name : Prop.Name;
        }

        public bool IsPrimaryKey { get; private set; }

        public bool IsManyToManyRelationship { get; private set; }

        public bool IsOneToManyRelationship { get; private set; }

        public bool IsManyToOneRelationship { get; private set; }

        public bool IsOneToOneRelationship { get; private set; }

        public bool IsReadOnly { get; private set; }

        public bool IsPublic { get; private set; }

        public bool IsSaveable => !IsReadOnly && IsPublic;

        public bool IsGenericDictionary { get; private set; }

        public bool IsEnumerable { get; private set; }

        public bool IsEnum { get; private set; }

        public bool IsBool { get; private set; }

        public bool IsValueType { get; private set; }

        public bool IsNumericType { get; private set; }

        public bool IsDateTime { get; private set; }

        public bool IsDateTimeOffset { get; private set; }

        public bool IsReferenceType { get; private set; }

        public bool IsString { get; private set; }

        public string ColumnName { get; private set; }

        public bool IsSoftDeletable { get; private set; }

        public object GetValue(object source)
        {
            return source.TryGetPropertyValue(Prop.Name);
        }

        public void SetValue(object source, object value)
        {
            source.TrySetPropertyValue(Prop.Name, value);
        }

        public void InitDictionary()
        {
            IsGenericDictionary = Prop.PropertyType.IsGenericDictionary();
        }

        private void InitEnumerable()
        {
            IsEnumerable = Prop.PropertyType.IsEnumerable() && typeof(string) != Prop.PropertyType;
        }

        private void InitValueType()
        {
            IsValueType = Prop.PropertyType.IsValueType;
        }

        private void InitEnum()
        {
            IsValueType = Prop.PropertyType.IsEnum;
        }

        private void InitBool()
        {
            IsBool = typeof(bool) == Prop.PropertyType;
        }

        private void InitDateTime()
        {
            IsDateTime = typeof(DateTime) == Prop.PropertyType;
        }

        private void InitDateTimeOffset()
        {
            IsDateTimeOffset = typeof(DateTimeOffset) == Prop.PropertyType;
        }
        private void InitNumericType()
        {
            IsNumericType = IsValueType
                && !(IsEnumerable || IsBool || IsDateTime || IsDateTimeOffset);
        }

        private void InitReferenceType()
        {
            IsReferenceType = Prop.PropertyType.IsClass
                || Prop.PropertyType.IsInterface;
        }

        private void InitString()
        {
            IsString = typeof(string) == Prop.PropertyType;
        }
    }
}
