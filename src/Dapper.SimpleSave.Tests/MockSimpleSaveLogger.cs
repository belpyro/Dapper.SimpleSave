﻿using System.Collections.Generic;
using Dapper.SimpleSave.Impl;

namespace Dapper.SimpleSave.Tests
{
    public class MockSimpleSaveLogger : BasicSimpleSaveLogger
    {
        private IList<IScript> _scripts = new List<IScript>();

        public IList<IScript> Scripts
        {
            get { return _scripts; }
        }

        public override void LogBuilt(IScript script)
        {
            _scripts.Add(script);
            base.LogPreExecution(script);
        }
    }
}
