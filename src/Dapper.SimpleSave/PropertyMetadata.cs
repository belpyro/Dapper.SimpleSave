using System;
using System.Reflection;
using Dapper.SimpleSave.Impl;

namespace Dapper.SimpleSave
{
    public class PropertyMetadata : BaseMetadata
    {
        public PropertyInfo Prop { get; set; }

        public PropertyMetadata(PropertyInfo prop) : base(prop)
        {
            Prop = prop;

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

        public bool IsPrimaryKey => HasAttribute<PrimaryKeyAttribute>();

        public bool IsManyToManyRelationship => HasAttribute<ManyToManyAttribute>();

        public bool IsOneToManyRelationship => HasAttribute<OneToManyAttribute>();

        public bool IsManyToOneRelationship => HasAttribute<ManyToOneAttribute>();

        public bool IsOneToOneRelationship => HasAttribute<OneToOneAttribute>();

        public bool IsReadOnly => HasAttribute<SimpleSaveIgnoreAttribute>() || ! Prop.CanWrite;

        public bool IsPublic => Prop.GetGetMethod().IsPublic;

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

        public string ColumnName => HasAttribute<ColumnAttribute>()
            ? GetAttribute<ColumnAttribute>().Name
            : Prop.Name;

        public object GetValue(object source)
        {
            return Prop.GetGetMethod().Invoke(source, new object[0]);
        }

        public void SetValue(object source, object value)
        {
            Prop.GetSetMethod().Invoke(source, new[] {value});
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
            IsBool = typeof (bool) == Prop.PropertyType;
        }

        private void InitDateTime()
        {
            IsDateTime = typeof (DateTime) == Prop.PropertyType;
        }

        private void InitDateTimeOffset()
        {
            IsDateTimeOffset = typeof (DateTimeOffset) == Prop.PropertyType;
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
            IsString = typeof (string) == Prop.PropertyType;
        }
    }
}