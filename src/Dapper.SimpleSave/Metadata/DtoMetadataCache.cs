using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dapper.SimpleSave.Impl;

namespace Dapper.SimpleSave.Metadata
{
    public class DtoMetadataCache
    {
        private readonly IDictionary<Type, DtoMetadata> _metadata = new ConcurrentDictionary<Type, DtoMetadata>();

        public DtoMetadata GetMetadataFor(Type type)
        {
            if (type == typeof(string))
            {
                throw new ArgumentException("DtoMetadata retrieval is not supported for strings.", nameof(type));
            }

            DtoMetadata data;
            _metadata.TryGetValue(type, out data);
            if (null != data) return data;

            data = new DtoMetadata(type);
            _metadata.Add(type, data);
            return data;
        }

        public DtoMetadata GetMetadataFor<T>()
        {
            return GetMetadataFor(typeof(T));
        }

        public DtoMetadata GetValidatedMetadataFor(Type type)
        {
            var metadata = GetMetadataFor(type);
            DtoMetadataValidator.ValidateAsCompatibleTable(metadata);
            return metadata;
        }

        public DtoMetadata GetValidatedMetadataFor<T>()
        {
            return GetValidatedMetadataFor(typeof(T));
        }

        public bool HasMetaDataFor(Type type)
        {
            return _metadata.ContainsKey(type);
        }
    }
}
