﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.SimpleSave.Impl {
    public abstract class BaseCommand
    {
        public string TableName { get; protected set; }

        public int? PrimaryKey { get; protected set; }

        public string PrimaryKeyColumn { get; protected set; }
 

    }
}
