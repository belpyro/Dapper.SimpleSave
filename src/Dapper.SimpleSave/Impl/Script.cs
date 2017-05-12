﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using Dapper.SimpleSave.Metadata;

namespace Dapper.SimpleSave.Impl
{
    public class Script : IScript
    {
        public Script()
        {
            Buffer = new StringBuilder();
            Parameters = new ExpandoObject();
            WireUpActions = new List<Action>();
        }

        public StringBuilder Buffer { get; private set; }
        public IDictionary<string, object> Parameters { get; private set; }
        public IList<Action> WireUpActions { get; private set; } 
        public object Config { get { return Parameters; } }
        public object InsertedValue { get; set; }
        public DtoMetadata InsertedValueMetadata { get; set; }
    }
}
