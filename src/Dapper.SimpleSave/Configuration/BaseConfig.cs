using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dapper.SimpleSave.Attributes;
using Dapper.SimpleSave.Metadata;

namespace Dapper.SimpleSave.Configuration
{
    public sealed class BaseConfig<T> : IConfiguration
    {
        public string TableName { get; set; }

        public IPropertyMetadataExt Key { get; set; }

        public Type DtoType { get; } = typeof(T);

        public ReferenceData ReferenceData { get; set; }

        public IList<MemberRelation> Relations { get; } = new List<MemberRelation>();

        public IDictionary<string, IPropertyMetadataExt> Properties { get; } = new ConcurrentDictionary<string, IPropertyMetadataExt>();

        public IDictionary<string, IPropertyMetadataExt> WritableProperties => Properties.Where(
            x => !x.Value.IsReadOnly).ToDictionary(x => x.Key, x => x.Value);
    }
}