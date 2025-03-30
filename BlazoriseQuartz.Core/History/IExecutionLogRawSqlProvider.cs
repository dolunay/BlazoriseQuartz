namespace BlazoriseQuartz.Core.History
{
    public interface IExecutionLogRawSqlProvider
    {
        string DeleteLogsByDays { get; }
    }
}