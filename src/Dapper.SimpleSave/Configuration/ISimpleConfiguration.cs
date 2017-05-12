namespace Dapper.SimpleSave.Configuration
{
    public interface ISimpleConfiguration
    {
        /// <summary>
        /// Indicates whether or not query parameters should be included in log output. Off by
        /// default and should not be switched on unless you are certain your query parameters
        /// do not contain sensitive information.
        /// </summary>
        bool IncludeParametersInLog { get; set; }

        /// <summary>
        /// Indicates whether or not query parameters should be included in exceptions. Off by
        /// default and should not be switched on unless you are certain your query parameters
        /// do not contain sensitive information.
        /// </summary>
        bool IncludeParametersInException { get; set; }

        bool AutoGenerateTableName { get; set; }

        bool PluralizeTableName { get; set; }

        long QueryDurationMillisExceptionThrowThreshold { get; set; }

        int RowCountWarningEmitThreshold { get; set; }

        int RowCountErrorEmitThreshold { get; set; }

        int RowCountExceptionThrowThreshold { get; set; }

        long QueryDurationMillisWarningEmitThreshold { get; set; }

        long QueryDurationMillisErrorEmitThreshold { get; set; }
    }
}