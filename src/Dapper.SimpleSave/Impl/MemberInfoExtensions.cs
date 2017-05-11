using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapper.SimpleSave.Impl
{
    public static class MemberInfoExtensions
    {
        public static IDictionary<Type, Attribute> GetAttributesDict(this MemberInfo member)
        {
            var target = new ConcurrentDictionary<Type, Attribute>();
            foreach (var attr in member.GetCustomAttributes(true).OfType<Attribute>())
            {
                target[attr.GetType()] = attr;
            }
            return target;
        }
    }
}
