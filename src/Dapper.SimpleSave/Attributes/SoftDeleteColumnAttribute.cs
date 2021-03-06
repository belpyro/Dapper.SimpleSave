﻿using System;

namespace Dapper.SimpleSave.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SoftDeleteColumnAttribute : Attribute
    {
        public SoftDeleteColumnAttribute() : this(false)
        {
        }

        public SoftDeleteColumnAttribute(bool trueIndicatesDeleted)
        {
            TrueIndicatesDeleted = trueIndicatesDeleted;
        }

        public bool TrueIndicatesDeleted { get; private set; }
    }
}
