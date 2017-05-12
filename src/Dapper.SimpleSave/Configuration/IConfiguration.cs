using System;
using System.Collections.Generic;
using Dapper.SimpleSave.Attributes;
using Dapper.SimpleSave.Metadata;

namespace Dapper.SimpleSave.Configuration
{
    public interface IConfiguration
    {
        string TableName { get; set; }

        IPropertyMetadataExt Key { get; set; }

        Type DtoType { get; }

        ReferenceData ReferenceData { get; set; }

        IDictionary<string, IPropertyMetadataExt> Properties { get; }

        IDictionary<string, IPropertyMetadataExt> WritableProperties { get; }
    }
}