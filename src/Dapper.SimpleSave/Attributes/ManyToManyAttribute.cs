﻿using System;

namespace Dapper.SimpleSave.Attributes
{
    public class ManyToManyAttribute : Attribute
    {
        public string SchemaQualifiedLinkTableName { get; private set; }

        public ManyToManyAttribute(string schemaQualifiedLinkTableName)
        {
            SchemaQualifiedLinkTableName = schemaQualifiedLinkTableName;
        }
    }
}
