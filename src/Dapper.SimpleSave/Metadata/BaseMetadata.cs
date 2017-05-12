using System;
using System.Collections.Generic;
using System.Reflection;
using Dapper.SimpleSave.Impl;

namespace Dapper.SimpleSave.Metadata
{
    public abstract class BaseMetadata
    {
        private readonly IDictionary<Type, Attribute> _attributes;

        protected BaseMetadata(MemberInfo member)
        {
            _attributes = member.GetAttributesDict();
        }

        public bool HasAttribute<T>() where T : Attribute
        {
            return _attributes.ContainsKey(typeof(T));
        }

        //todo remove
        public T GetAttribute<T>() where T : Attribute
        {
            Attribute attr;
            _attributes.TryGetValue(typeof(T), out attr);
            return (T)attr;
        }
    }
}
