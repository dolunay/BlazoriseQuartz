using System;
using Quartz;

namespace BlazoriseQuartz.Core.Helpers
{
    public static class CronExpressionHelper
    {
        public static bool IsValidExpression(string cronExpression)
        {
            return CronExpression.IsValidExpression(cronExpression);
        }
    }
}

