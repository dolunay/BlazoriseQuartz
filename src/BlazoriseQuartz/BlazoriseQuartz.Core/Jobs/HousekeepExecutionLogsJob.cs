using System;
using BlazoriseQuartz.Core.History;
using Microsoft.Extensions.Options;
using Quartz;
using BlazoriseQuartz.Jobs.Abstractions;

namespace BlazoriseQuartz.Core.Jobs
{
    public class HousekeepExecutionLogsJob : IJob
    {
        private readonly IExecutionLogStore _logStore;
        private readonly BlazoriseQuartzCoreOptions _options;

        public HousekeepExecutionLogsJob(IExecutionLogStore logStore,
            IOptions<BlazoriseQuartzCoreOptions> options)
        {
            _logStore = logStore;
            _options = options.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var count = await _logStore.DeleteLogsByDays(_options.ExecutionLogsDaysToKeep);
                context.Result = $"Deleted {count} record(s)";
                context.SetIsSuccess(true);
            }
            catch (Exception ex)
            {
                throw new JobExecutionException("Failed to delete execution logs", ex);
            }
        }
    }
}

