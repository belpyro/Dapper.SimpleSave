namespace Dapper.SimpleSave
{
    // todo make configuration read-only
    public sealed class BasicConfiguration : ISimpleConfiguration
    {
        public bool IncludeParametersInLog { get; set; }
        public bool IncludeParametersInException { get; set; }
        public bool AutoGenerateTableName { get; set; }
        public bool PluralizeTableName { get; set; }
        public long QueryDurationMillisExceptionThrowThreshold { get; set; } = 0; //  I.e., don't ever thrown an exception
        public int RowCountWarningEmitThreshold { get; set; } = 1000;
        public int RowCountErrorEmitThreshold { get; set; } = 2000;
        public int RowCountExceptionThrowThreshold { get; set; } = 0; //  I.e., don't ever thrown an exception
        public long QueryDurationMillisWarningEmitThreshold { get; set; } = 2000;
        public long QueryDurationMillisErrorEmitThreshold { get; set; } = 4000;
    }
}