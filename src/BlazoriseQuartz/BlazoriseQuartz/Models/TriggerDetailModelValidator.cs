using Blazorise;
using BlazoriseQuartz.Core.Helpers;
using BlazoriseQuartz.Core.Models;
using BlazoriseQuartz.Core.Services;

namespace BlazoriseQuartz.Models
{
	public class TriggerDetailModelValidator : ITriggerDetailModelValidator
	{
		private readonly ISchedulerService _schedulerSvc;

		public TriggerDetailModelValidator(ISchedulerService schSvc)
		{
			_schedulerSvc = schSvc;
		}

		/// <summary>
		/// Returns true if Days of week validation is successful
		/// </summary>
		/// <param name="triggerModel"></param>
		/// <returns></returns>
		public bool ValidateDaysOfWeek(TriggerDetailModel triggerModel)
		{
			if (triggerModel.TriggerType != TriggerType.Daily)
				return true;

			foreach (var val in triggerModel.DailyDayOfWeek)
			{
				if (val)
				{
					return true;
				}
			}

			return false;
		}


		public async Task<string?> ValidateTriggerName(string name, TriggerDetailModel triggerModel,
			Key? originalKey)
		{
			if (string.IsNullOrEmpty(name))
				return "Trigger name is required";

			if (originalKey != null &&
				originalKey.Equals(name, triggerModel.Group))
				return null;

			var exists = await _schedulerSvc.ContainsTriggerKey(name, triggerModel.Group);

			if (exists)
				return "Trigger name and group already defined. Please choose another name.";

			return null;
		}

		public string? ValidateTime(TimeSpan? start, object end, string errorMessage)
		{
			var endSpan = (TimeSpan?)end;
			if (start.HasValue && endSpan.HasValue)
			{
				if (start.Value > endSpan.Value)
					return errorMessage;
			}

			return null;
		}

		public void ValidateTime(TimeSpan? start, ValidatorEventArgs e)
		{
			var endSpan = (TimeSpan?)e.Value;
			if (start.HasValue && endSpan.HasValue)
			{
				if (start.Value > endSpan.Value)
				{
					e.Status = ValidationStatus.Error;
					return;
				}
			}

			e.Status = ValidationStatus.Success;
		}

		public string? ValidateFirstLastDateTime(TriggerDetailModel model, string errorMessage)
		{
			if (!model.StartDate.HasValue ||
				!model.EndDate.HasValue)
				return null;

			var start = model.StartDate.Value.Add(model.StartTimeSpan ?? TimeSpan.Zero);
			var end = model.EndDate.Value.Add(model.EndTimeSpan ?? TimeSpan.Zero);

			if (start > end)
				return errorMessage;

			return null;
		}

		public string? ValidateCronExpression(string? cronExpression)
		{
			if (string.IsNullOrEmpty(cronExpression))
				return "Cron expression is required";

			if (!CronExpressionHelper.IsValidExpression(cronExpression))
				return "Check cron expression";

			return null;
		}

		public void ValidateFirstLastDateTime(TriggerDetailModel model, ValidatorEventArgs e)
		{
			if (!model.StartDate.HasValue || !model.EndDate.HasValue)
			{
				e.Status = ValidationStatus.None;
				return;
			}
			else
			{
				var start = model.StartDate.Value.Add(model.StartTimeSpan ?? TimeSpan.Zero);
				var end = model.EndDate.Value.Add(model.EndTimeSpan ?? TimeSpan.Zero);

				if (start > end)
					e.Status = ValidationStatus.Error;
				return;
			}
			e.Status = ValidationStatus.Success;
		}

		public void ValidateCronExpression(ValidatorEventArgs eventArgs)
		{
			var cronExpression = Convert.ToString(eventArgs.Value);
			if (string.IsNullOrEmpty(cronExpression))
			{
				eventArgs.ErrorText = "Cron expression is required";
				eventArgs.Status = ValidationStatus.Error;
				return;
			}

			if (!CronExpressionHelper.IsValidExpression(cronExpression))
			{
				eventArgs.ErrorText = "Check cron expression";
				eventArgs.Status = ValidationStatus.Error;
				return;
			}

			eventArgs.Status = ValidationStatus.Success;
		}

		public async Task ValidateTriggerName(ValidatorEventArgs eventArgs, TriggerDetailModel triggerModel, Key? triggerKey)
		{
			var name = Convert.ToString(eventArgs.Value);
			if (string.IsNullOrEmpty(name))
			{
				eventArgs.ErrorText = "Trigger name is required";
				eventArgs.Status =ValidationStatus.Error;
				return;
			}

			if (triggerKey != null && triggerKey.Equals(name, triggerModel.Group))
			{
				eventArgs.Status = ValidationStatus.None;
				return;
			}
			var exists = await _schedulerSvc.ContainsTriggerKey(name, triggerModel.Group);

			if (exists)
			{
				eventArgs.ErrorText = "Trigger name and group already defined. Please choose another name.";
				eventArgs.Status = ValidationStatus.Error;
				return;
			}

			eventArgs.Status = ValidationStatus.Success;
		}
	}
}

