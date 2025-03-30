using System;
using System.Collections.Immutable;
using Blazorise;
using BlazoriseQuartz.Core;
using BlazoriseQuartz.Core.Models;
using BlazoriseQuartz.Core.Services;
using Microsoft.AspNetCore.Components;
using BlazoriseQuartz.Extensions;
using BlazoriseQuartz.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlazoriseQuartz.Components
{
	public partial class BlazoriseTrigger : ComponentBase
	{
		const string CRON_HELP_TEXT = "Cron expression: seconds minutes hours day-of-month month day-of-week year";
		[Inject] private ISchedulerDefinitionService SchedulerDefSvc { get; set; } = null!;
		[Inject] private ISchedulerService SchedulerSvc { get; set; } = null!;
		[Inject] private ITriggerDetailModelValidator Validator { get; set; } = null!;
		[Inject] private IModalService DialogSvc { get; set; } = null!;
		[Inject] private IMessageService MessageSvc { get; set; } = null!;

		[Parameter]
		[EditorRequired]
		public TriggerDetailModel TriggerDetail { get; set; } = new();

		[Parameter] public bool IsValid { get; set; }

		[Parameter] public EventCallback<bool> IsValidChanged { get; set; }

		private ISet<TriggerType> ExcludedTriggerTypeChoices = new HashSet<TriggerType> { TriggerType.Unknown, TriggerType.Calendar };

		private IEnumerable<SelectListItem>? ExistingTriggerGroups;

		private string? CronDescription = CRON_HELP_TEXT;
		private Form _form = null!;
		private Validations _validations = null!;
		private bool _isDaysOfWeekValid = true;
		private IReadOnlyCollection<SelectListItem>? _calendars;
		private IReadOnlyCollection<TimeZoneInfo>? _timeZones;
		private TimePicker<TimeSpan?> _endDailyTimePicker = null!;
		private DatePicker<DateTime?> _endDatePicker = null!;
		private Key? OriginalTriggerKey { get; set; }

		private Dictionary<TriggerType, string> TriggerTypeIcons = new()
		{
			{ TriggerType.Cron, TriggerType.Cron.GetTriggerTypeIcon() },
			{ TriggerType.Daily, TriggerType.Daily.GetTriggerTypeIcon() },
			{ TriggerType.Simple, TriggerType.Simple.GetTriggerTypeIcon() },
			{ TriggerType.Calendar, TriggerType.Calendar.GetTriggerTypeIcon() },
		};

		protected override void OnInitialized()
		{
			OriginalTriggerKey = new(TriggerDetail.Name, TriggerDetail.Group);
			Task.Run(GetTimeZones);
		}

		public static void DailyDayOfWeekValidation(ValidatorEventArgs e)
		{
			if (e.Value != null)
			{
				var items = e.Value as bool[];
				if (items != null && items.Length > 0)
				{
					if (!items.Any())
					{
						e.Status = ValidationStatus.Error;
						return;
					}
					e.Status = ValidationStatus.Success;
					return;
				}
			}
			e.Status = ValidationStatus.None;
		}

		private async Task OnCronExpressionInputElapsed(string? cronExpression)
		{
			try
			{
				CronDescription = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cronExpression);
			}
			catch
			{
				CronDescription = CRON_HELP_TEXT;
			}

			await Task.CompletedTask;
		}

		private async Task SetCronExpression(string? cronExpression)
		{
			TriggerDetail.CronExpression = cronExpression;

			await Task.CompletedTask;
		}

		private async Task GetTriggerGroups()
		{
			ExistingTriggerGroups ??= (await SchedulerSvc.GetTriggerGroups()).Select(s => new SelectListItem(s, s));
		}

		//private async Task<IEnumerable<string>> SearchTriggerGroup(string value, CancellationToken cancellationToken)
		//{
		//    if (ExistingTriggerGroups == null)
		//    {
		//        ExistingTriggerGroups = await SchedulerSvc.GetTriggerGroups();
		//    }

		//    if (string.IsNullOrEmpty(value))
		//        return ExistingTriggerGroups;

		//    var matches = ExistingTriggerGroups
		//        .Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase))
		//        .ToList();

		//    if (matches.All(x => x != value))
		//        matches.Add(value);

		//    return matches;
		//}

		private async Task OnShowSampleCron()
		{
			var options = new ModalInstanceOptions
			{
				Size = ModalSize.Default
			};

			await DialogSvc.Show<CronSamplesDialog>("Sample Cron Expressions", p =>
			{
				p.Add("Save", (Delegate)SetCronExpression);
			}, options);
		}

		private async Task GetTimeZones()
		{
			_timeZones ??= await Task.Run(TimeZoneInfo.GetSystemTimeZones);
		}

		async Task<IEnumerable<TimeZoneInfo>> SearchTimeZoneInfo(string value, CancellationToken cancellationToken)
		{
			await Task.CompletedTask;

			var tzList = TimeZoneInfo.GetSystemTimeZones();

			if (string.IsNullOrEmpty(value))
			{
				return tzList;
			}

			return tzList.Where(x => x.DisplayName.Contains(value, StringComparison.InvariantCultureIgnoreCase));
		}

		private async Task GetCalendars()
		{
			_calendars ??= (await SchedulerSvc.GetCalendarNames()).Select(s => new SelectListItem(s, s)).ToImmutableList();
		}

		//async Task<IEnumerable<string>> SearchCalendars(string value, CancellationToken cancellationToken)
		//{
		//    if (_calendars == null)
		//    {
		//        _calendars = await SchedulerSvc.GetCalendarNames();
		//    }

		//    return _calendars.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
		//}

		private void OnSetValidationStatusChanged(ValidationsStatusChangedEventArgs eventArgs)
		{
			var value = eventArgs.Status == ValidationStatus.Success;
			if (IsValid == value)
				return;
			IsValid = value;
			IsValidChanged.InvokeAsync(value).RunSynchronously();
		}

		private void OnSetIsValid(bool value)
		{
			if (IsValid == value)
				return;
			IsValid = value;
			IsValidChanged.InvokeAsync(value).RunSynchronously();
		}

		public async Task Validate()
		{
			var isValid = await _validations.ValidateAll();

			_isDaysOfWeekValid = Validator.ValidateDaysOfWeek(TriggerDetail);
			if (isValid)
				OnSetIsValid(_isDaysOfWeekValid);

			// if daily trigger does not have end time, assign end time
			if (TriggerDetail is { TriggerType: TriggerType.Daily, EndDailyTime: null })
			{
				TriggerDetail.EndDailyTime = TriggerDetail.StartDailyTime;
			}
		}

		//async Task OnStartDailyTimeChanged(TimeSpan? time)
		//{
		//    TriggerDetail.StartDailyTime = time;
		//    await _endDailyTimePicker.Validate();
		//}

		//async Task OnStartTimeChanged(TimeSpan? time)
		//{
		//    TriggerDetail.StartTimeSpan = time;
		//    await _endDatePicker.Validate();
		//}

		//async Task OnStartDateChanged(DateTime? time)
		//{
		//    TriggerDetail.StartDate = time;
		//    await _endDatePicker.Validate();
		//}

		public async Task AddDataMap(DataMapItemModel dataMap)
		{
			if (dataMap is { Key: not null, Value: not null })
				TriggerDetail.TriggerDataMap.Add(dataMap.Key, dataMap.Value);
			else
			{
				// TODO print error message. Data map is null
			}

			await Task.CompletedTask;
		}

		private async Task OnAddDataMap()
		{
			var options = new ModalInstanceOptions
			{
				Size = ModalSize.Small
			};

			await DialogSvc.Show<JobDataMapDialog>("Add Data Map", p =>
			{
				p.Add("JobDataMap", new Dictionary<string, object>(TriggerDetail.TriggerDataMap, StringComparer.OrdinalIgnoreCase));
				p.Add("Save", (Delegate)AddDataMap);
			}, options);
		}

		public async Task UpdateDataMap(DataMapItemModel dataMap)
		{
			if (dataMap is { Key: not null, Value: not null })
			{
				TriggerDetail.TriggerDataMap[dataMap.Key] = dataMap.Value;
			}
			else
			{
				// TODO print error message. Data map is null
			}

			await Task.CompletedTask;
		}

		private async Task OnEditDataMap(KeyValuePair<string, object> item)
		{
			var options = new ModalInstanceOptions
			{
				Size = ModalSize.Small
			};

			await DialogSvc.Show<JobDataMapDialog>("Edit Data Map", p =>
			{
				p.Add("ExistingDataMap", TriggerDetail.TriggerDataMap);
				p.Add("DataMapItem", new DataMapItemModel(item));
				p.Add("IsEditMode", true);
				p.Add("Save", (Delegate)UpdateDataMap);
			}, options);
		}

		private async Task OnCloneDataMap(KeyValuePair<string, object> item)
		{
			var options = new ModalInstanceOptions
			{
				Size = ModalSize.Small
			};
			var index = 1;
			var key = item.Key + index++;

			while (TriggerDetail.TriggerDataMap.ContainsKey(key))
			{
				if (index == int.MaxValue)
				{
					key = string.Empty;
					break;
				}

				key = item.Key + index++;
			}
			var clonedItem = new KeyValuePair<string, object>(key, item.Value);
			await DialogSvc.Show<JobDataMapDialog>("Add Data Map", p =>
			{
				p.Add("ExistingDataMap", new Dictionary<string, object>(TriggerDetail.TriggerDataMap, StringComparer.OrdinalIgnoreCase));
				p.Add("DataMapItem", new DataMapItemModel(clonedItem));
				p.Add("Save", (Delegate)UpdateDataMap);
			}, options);
		}

		private async Task OnDeleteDataMap(KeyValuePair<string, object> item)
		{
			bool? yes = await MessageSvc.Confirm(
				"Confirm Delete",
				$"Do you want to delete '{item.Key}'?",
				o =>
				{
					o.OkButtonText = "Yes";
					o.CancelButtonText = "No";
				});

			if (yes == null || !yes.Value)
			{
				return;
			}

			TriggerDetail.TriggerDataMap.Remove(item);
		}
	}
}

