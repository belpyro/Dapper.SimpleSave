using System;

namespace Dapper.SimpleSave.Configuration
{
    public class MemberRelation
    {
        public Type To { get; set; }

        public string ColumnName { get; set; }

        public string PropertyName { get; set; }
    }
}