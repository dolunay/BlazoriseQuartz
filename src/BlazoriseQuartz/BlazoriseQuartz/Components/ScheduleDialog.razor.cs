using Blazorise;
using Blazorise.Icons.Material;
using BlazoriseQuartz.Core.Models;
using BlazoriseQuartz.Core.Services;
using BlazoriseQuartz.Pages.BlazoriseQuartzUI.Schedules;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazoriseQuartz.Components;

public partial class ScheduleDialog
{
	[Inject] public IModalService ModalService { get; set; } = null!;
	[Inject] private ISchedulerService SchedulerSvc { get; set; } = null!;
	[Inject] private ILogger<Schedules> _logger { get; set; } = null!;
	[Inject] private INotificationService Snackbar { get; set; } = null!;
	[Parameter] public JobDetailModel JobDetail { get; set; } = new();
	[Parameter] public TriggerDetailModel TriggerDetail { get; set; } = new();
	[Parameter] public bool IsNew { get; set; }
	[Parameter] public Key JobKey { get; set; }
	[Parameter] public Key TriggerKey { get; set; }
	[Parameter] public bool IsReadOnlyJobDetail { get; set; }
	[Parameter] public ScheduleDialogTab SelectedTab { get; set; } = ScheduleDialogTab.Job;

	private bool _jobDetailIsValid;
	private bool _triggerDetailIsValid;
	private string _nextText = "Next";
	private string? _nextIcon = MaterialIcons.NavigateNext;
	private BlazoriseJob _jobPanel = null!;
	private BlazoriseTrigger _triggerPanel = null!;

	protected override void OnInitialized()
	{
		if (SelectedTab == ScheduleDialogTab.Trigger)
		{
			_jobDetailIsValid = true;
			_nextText = "Save";
			_nextIcon = null;
		}
	}

	private async Task OnSelectedTabChanged(ScheduleDialogTab tab)
	{
		if (SelectedTab == tab)
			return;

		// validate before change tab
		if (SelectedTab == ScheduleDialogTab.Job)
		{
			await _jobPanel.Validate();
			if (!_jobDetailIsValid)
				return;
		}

		SelectedTab = tab;

		// update text
		if (SelectedTab == ScheduleDialogTab.Job)
		{
			_nextText = "Next";
			_nextIcon = MaterialIcons.NavigateNext;
		}
		else if (SelectedTab == ScheduleDialogTab.Trigger)
		{
			if (string.IsNullOrEmpty(TriggerDetail.Name) &&
				!string.IsNullOrEmpty(JobDetail.Name))
			{
				// use job name as trigger name when trigger name not yet specified
				// determine if trigger name can be used
				var exists = await SchedulerSvc.ContainsTriggerKey(JobDetail.Name, TriggerDetail.Group);
				if (!exists)
					TriggerDetail.Name = JobDetail.Name;
			}

			_nextText = "Save";
			_nextIcon = null;
		}
	}

	private async Task OnBack()
	{
		await OnSelectedTabChanged(ScheduleDialogTab.Job);
	}

	private async Task OnSubmit()
	{
		if (SelectedTab == ScheduleDialogTab.Job)
		{
			await OnSelectedTabChanged(ScheduleDialogTab.Trigger);
			return;
		}

		await _triggerPanel.Validate();
		if (!_jobDetailIsValid || !_triggerDetailIsValid)
		{
			return;
		}

		if (IsNew)
		{
			// create schedule
			try
			{
				await SchedulerSvc.CreateSchedule(JobDetail, TriggerDetail);
			}
			catch (Exception ex)
			{
				await Snackbar.Error($"Failed to create new schedule. {ex.Message}");
				_logger.LogError(ex, "Failed to create new schedule.");
				// TODO show schedule dialog again?
			}
		}
		else
		{
			try
			{
				await SchedulerSvc.UpdateSchedule(JobKey, TriggerKey, JobDetail, TriggerDetail);
			}
			catch (Exception ex)
			{
				await Snackbar.Error($"Failed to update schedule. {ex.Message}");
				_logger.LogError(ex, "Failed to update schedule.");
				// TODO display the dialog again?
			}
		}

		await ModalService.Hide();
	}

	protected async Task OnCancel()
	{
		await ModalService.Hide();
	}
}