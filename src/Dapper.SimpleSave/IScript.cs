using System;
using System.Collections.Generic;
using System.Text;
using Dapper.SimpleSave.Metadata;

namespace Dapper.SimpleSave
{
    public interface IScript {
        StringBuilder Buffer { get; }
        IDictionary<string, object> Parameters { get; }
        IList<Action> WireUpActions { get; }
        object Config { get; }
        object InsertedValue { get; set; }
        DtoMetadata InsertedValueMetadata { get; set; }
    }
}