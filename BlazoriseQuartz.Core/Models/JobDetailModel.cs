using System;
using System.ComponentModel.DataAnnotations;
namespace BlazoriseQuartz.Core.Models
{
    public class JobDetailModel
    {
        [Required(ErrorMessage = "Job Name is required")]
        public string Name { get; set; } = string.Empty;

        public string Group { get; set; } = Constants.DEFAULT_GROUP;
        public string? Description { get; set; }
        public Type? JobClass { get; set; }
        public IDictionary<string, object> JobDataMap { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Flag indicate whether Job should stay in scheduler even there are no more triggers assgined to it.
        /// </summary>
        public bool IsDurable { get; set; }
    }
}

