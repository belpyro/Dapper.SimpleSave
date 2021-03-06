﻿using System;

namespace Dapper.SimpleSave.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyReferenceAttribute : Attribute
    {
        public Type ReferencedDto { get; private set; }

        public ForeignKeyReferenceAttribute(Type referencedDto)
        {
            ReferencedDto = referencedDto;
        }
    }
}
