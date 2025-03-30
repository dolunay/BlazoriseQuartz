using System;
namespace BlazoriseQuartz.Core.Models
{
    public class ScheduleJobFilter : ICloneable
    {
        public bool IncludeSystemJobs { get; set; } = false;

        public object Clone()
        {
            return (ScheduleJobFilter)this.MemberwiseClone();
        }
    }
}

