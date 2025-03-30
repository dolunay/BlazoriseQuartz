using Blazorise;
using Blazorise.DataGrid;
using BlazoriseQuartz.Components;
using BlazoriseQuartz.Core;
using BlazoriseQuartz.Core.Data;
using BlazoriseQuartz.Core.Events;
using BlazoriseQuartz.Core.Models;
using BlazoriseQuartz.Core.Services;
using BlazoriseQuartz.Extensions;
using BlazoriseQuartz.Jobs.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Logging;
using System.Collections.ObjectModel;

namespace BlazoriseQuartz.Pages.BlazoriseQuartzUI.Schedules
{
    public partial class Schedules : ComponentBase, IDisposable
    {
        [Inject] private ISchedulerService SchedulerSvc { get; set; } = null!;
        [Inject] private ISchedulerListenerService SchedulerListenerSvc { get; set; } = null!;
        [Inject] private IExecutionLogService ExecutionLogSvc { get; set; } = null!;
        [Inject] private IModalService DialogSvc { get; set; } = null!;
        [Inject] private IMessageService MessageSvc { get; set; } = null!;
        [Inject] private ILogger<Schedules> Logger { get; set; } = null!;
        [Inject] private INotificationService Snackbar { get; set; } = null!;

        private ObservableCollection<ScheduleModel> ScheduledJobs { get; set; } = new();
        private string? _searchJobKeyword;
        private DataGrid<ScheduleModel> _scheduleDataGrid=null!;

        private bool _openFilter;

        private ScheduleJobFilter _filter = new();
        private ScheduleJobFilter _origFilter = new();

        internal bool IsEditActionDisabled(ScheduleModel model) => (model.JobStatus == JobStatus.NoSchedule ||
            model.JobStatus == JobStatus.Error ||
            model.JobStatus == JobStatus.Running ||
            model.JobGroup == Constants.SYSTEM_GROUP);

        internal bool IsRunActionDisabled(ScheduleModel model) => (model.JobStatus == JobStatus.NoSchedule ||
                                            model.JobStatus == JobStatus.NoTrigger);

        internal bool IsPauseActionDisabled(ScheduleModel model) => (model.JobStatus == JobStatus.NoSchedule ||
                                            model.JobStatus == JobStatus.Error ||
                                            model.JobStatus == JobStatus.NoTrigger);

        internal bool IsTriggerNowActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule ||
            model.JobStatus == JobStatus.Error ||
            model.JobStatus == JobStatus.Running;

        internal bool IsAddTriggerActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule ||
            model.JobStatus == JobStatus.Error ||
            model.JobGroup == Constants.SYSTEM_GROUP;

        internal bool IsCopyActionDisabled(ScheduleModel model) => (model.JobStatus == JobStatus.NoSchedule ||
            model.JobStatus == JobStatus.Error ||
            model.JobGroup == Constants.SYSTEM_GROUP);

        internal bool IsHistoryActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule;

        internal bool IsDeleteActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.Running;

        // private TableGroupDefinition<ScheduleModel> _groupDefinition = new()
        // {
        //     GroupName = string.Empty,
        //     Indentation = false,
        //     Expandable = true,
        //     Selector = (e) => e.JobGroup
        // };

        readonly Func<ScheduleModel, object> _groupDefinition = x => x.JobGroup;

        static string GetTooltipText(ScheduleModel context)
        {
	        var str = "<div style='max-width: 220px; overflow-wrap: break-word;'>";
	        if (!string.IsNullOrEmpty(context.ExceptionMessage))
		        str += "Job has error." + context.ExceptionMessage;
	        else
		        str += "Job has error.";
	        str+="</div>";
	        return str;
        }

        static string ExceptionMessageToolTipText(ScheduleModel context) =>
	        $"<div style='max-width: 220px; overflow-wrap: break-word;'>{context.ExceptionMessage}</div>";

        static string TriggerDetailToolTipText(ScheduleModel context) =>
	        $"<div style='max-width: 220px; overflow-wrap: break-word;'>{context.TriggerDetail?.ToSummaryString()}</div>";

        protected override async Task OnInitializedAsync()
        {
            RegisterEventListeners();
            await RefreshJobs();
        }

        private void UnRegisterEventListeners()
        {
            SchedulerListenerSvc.OnJobToBeExecuted -= SchedulerListenerSvc_OnJobToBeExecuted;
            SchedulerListenerSvc.OnJobScheduled -= SchedulerListenerSvc_OnJobScheduled;
            SchedulerListenerSvc.OnJobWasExecuted -= SchedulerListenerSvc_OnJobWasExecuted;
            SchedulerListenerSvc.OnTriggerFinalized -= SchedulerListenerSvc_OnTriggerFinalized;
            SchedulerListenerSvc.OnJobDeleted -= SchedulerListenerSvc_OnJobDeleted;
            SchedulerListenerSvc.OnJobUnscheduled -= SchedulerListenerSvc_OnJobUnscheduled;
            SchedulerListenerSvc.OnTriggerResumed -= SchedulerListenerSvc_OnTriggerResumed;
            SchedulerListenerSvc.OnTriggerPaused -= SchedulerListenerSvc_OnTriggerPaused;
        }

        private void RegisterEventListeners()
        {
            SchedulerListenerSvc.OnJobToBeExecuted += SchedulerListenerSvc_OnJobToBeExecuted;
            SchedulerListenerSvc.OnJobScheduled += SchedulerListenerSvc_OnJobScheduled;
            SchedulerListenerSvc.OnJobWasExecuted += SchedulerListenerSvc_OnJobWasExecuted;
            SchedulerListenerSvc.OnTriggerFinalized += SchedulerListenerSvc_OnTriggerFinalized;
            SchedulerListenerSvc.OnJobDeleted += SchedulerListenerSvc_OnJobDeleted;
            SchedulerListenerSvc.OnJobUnscheduled += SchedulerListenerSvc_OnJobUnscheduled;
            SchedulerListenerSvc.OnTriggerResumed += SchedulerListenerSvc_OnTriggerResumed;
            SchedulerListenerSvc.OnTriggerPaused += SchedulerListenerSvc_OnTriggerPaused;
        }

        private async void SchedulerListenerSvc_OnTriggerPaused(object? sender, EventArgs<TriggerKey> e)
        {
            var triggerKey = e.Args;

            await InvokeAsync(() =>
            {
                var model = FindScheduleModelByTrigger(triggerKey).SingleOrDefault();
                if (model != null)
                {
                    model.JobStatus = JobStatus.Paused;
                    StateHasChanged();
                }
            });
        }

        private async void SchedulerListenerSvc_OnTriggerResumed(object? sender, EventArgs<TriggerKey> e)
        {
            var triggerKey = e.Args;

            await InvokeAsync(() =>
            {
                var model = FindScheduleModelByTrigger(triggerKey).SingleOrDefault();
                if (model != null)
                {
                    model.JobStatus = JobStatus.Idle;
                    StateHasChanged();
                }
            });
        }

        private async void SchedulerListenerSvc_OnJobUnscheduled(object? sender, EventArgs<TriggerKey> e)
        {
            Logger.LogInformation("Job trigger {triggerKey} got unscheduled", e.Args);
            await OnTriggerRemoved(e.Args);
        }

        private async void SchedulerListenerSvc_OnJobDeleted(object? sender, EventArgs<JobKey> e)
        {
            var jobKey = e.Args;
            Logger.LogInformation("Delete all schedule job {jobKey}", jobKey);

            await InvokeAsync(() =>
            {
                var modelList = ScheduledJobs.Where(s => s.JobName == jobKey.Name &&
                        s.JobGroup == jobKey.Group).ToList();
                modelList.ForEach(s => ScheduledJobs.Remove(s));
            });
        }

        private async void SchedulerListenerSvc_OnTriggerFinalized(object? sender, EventArgs<ITrigger> e)
        {
            var triggerKey = e.Args.Key;
            Logger.LogInformation("Trigger {triggerKey} finalized", triggerKey);

            await OnTriggerRemoved(triggerKey);
        }

        private async Task OnTriggerRemoved(TriggerKey triggerKey)
        {
            await InvokeAsync(async () =>
            {
                ScheduleModel? model;
                try
                {
                    model = FindScheduleModelByTrigger(triggerKey).SingleOrDefault();
                }
                catch (Exception ex)
                {
                    await Snackbar.Warning($"Cannot update trigger status. Found more than one schedule with trigger {triggerKey}");
                    Logger.LogWarning(ex, "Cannot update trigger status. Found more than one schedule with trigger {triggerKey}", triggerKey);
                    return;
                }

                if (model is not null)
                {
                    if (model.JobName == null || model.JobStatus == JobStatus.Error)
                    {
                        // Just remove if no way to get job details
                        // if status is error, means get job details will throw exception
                        ScheduledJobs.Remove(model);
                    }
                    else
                    {
                        var jobDetail = await SchedulerSvc.GetJobDetail(model.JobName, model.JobGroup);

                        if (jobDetail is { IsDurable: true })
                        {
                            // see if similar job name already exists
                            var similarJobNameExists = ScheduledJobs.Any(s => s != model &&
                                s.JobName == model.JobName &&
                                s.JobGroup == model.JobGroup);
                            if (similarJobNameExists)
                            {
                                // delete this duplicate no trigger job
                                ScheduledJobs.Remove(model);
                            }
                            else
                            {
                                model.JobStatus = JobStatus.NoTrigger;
                                model.ClearTrigger();
                            }
                        }
                        else
                        {
                            model.JobStatus = JobStatus.NoSchedule;
                        }
                    }

                    StateHasChanged();
                }
            });
        }

        private async void SchedulerListenerSvc_OnJobWasExecuted(object? sender, JobWasExecutedEventArgs e)
        {
            var jobKey = e.JobExecutionContext.JobDetail.Key;
            var triggerKey = e.JobExecutionContext.Trigger.Key;

            await InvokeAsync(() =>
            {
                var model = FindScheduleModel(jobKey, triggerKey).SingleOrDefault();
                if (model is not null)
                {
                    model.PreviousTriggerTime = e.JobExecutionContext.FireTimeUtc;
                    model.NextTriggerTime = e.JobExecutionContext.NextFireTimeUtc;
                    model.JobStatus = JobStatus.Idle;
                    var isSuccess = e.JobExecutionContext.GetIsSuccess();
                    if (e.JobException != null)
                        model.ExceptionMessage = e.JobException.Message;
                    else if (isSuccess.HasValue && !isSuccess.Value)
                        model.ExceptionMessage = e.JobExecutionContext.GetReturnCodeAndResult();

                    StateHasChanged();
                }
            });
        }

        private async void SchedulerListenerSvc_OnJobScheduled(object? sender, EventArgs<ITrigger> e)
        {
            if (!_filter.IncludeSystemJobs && (e.Args.JobKey.Group == Constants.SYSTEM_GROUP ||
                e.Args.Key.Group == Constants.SYSTEM_GROUP))
            {
                // system job is not visible, skip this event
                return;
            }

            await InvokeAsync(async () =>
            {
                var model = await SchedulerSvc.GetScheduleModelAsync(e.Args);
                ScheduledJobs.Add(model);
            });
        }

        private async void SchedulerListenerSvc_OnJobToBeExecuted(object? sender, EventArgs<IJobExecutionContext> e)
        {
            var jobKey = e.Args.JobDetail.Key;
            var triggerKey = e.Args.Trigger.Key;

            await InvokeAsync(() =>
            {
                var model = FindScheduleModel(jobKey, triggerKey).SingleOrDefault();
                if (model is not null)
                {
                    model.JobStatus = JobStatus.Running;

                    StateHasChanged();
                }
            });
        }

        private IEnumerable<ScheduleModel> FindScheduleModelByTrigger(TriggerKey triggerKey)
        {
            return ScheduledJobs.Where(j => j.EqualsTriggerKey(triggerKey) &&
                j.JobStatus != JobStatus.NoSchedule &&
                j.JobStatus != JobStatus.NoTrigger);
        }

        private IEnumerable<ScheduleModel> FindScheduleModel(JobKey jobKey, TriggerKey? triggerKey)
        {
            return ScheduledJobs.Where(j => j.Equals(jobKey, triggerKey)
                && ((j.JobStatus != JobStatus.NoSchedule && j.JobStatus != JobStatus.NoTrigger)
                    || j is { JobStatus: JobStatus.Error, TriggerName: not null })
                );
        }

        private async Task RefreshJobs()
        {
            ScheduledJobs.Clear();

            var jobs = SchedulerSvc.GetAllJobsAsync(_filter);
            await foreach (var job in jobs)
            {
                ScheduledJobs.Add(job);
            }
            if (ScheduledJobs.Any())
                _scheduleDataGrid?.ExpandAllGroups();

            await UpdateScheduleModelsLastExecution();
        }

        private async Task UpdateScheduleModelsLastExecution()
        {
            var latestResult = new PageMetadata(0, 1);
            var scheduleJobType = new HashSet<LogType> { LogType.ScheduleJob };

            foreach (var schModel in ScheduledJobs)
            {
                if (string.IsNullOrEmpty(schModel.JobName))
                    continue;

                var latestLogList = await ExecutionLogSvc.GetLatestExecutionLog(schModel.JobName, schModel.JobGroup,
                    schModel.TriggerName, schModel.TriggerGroup, latestResult,
                    logTypes: scheduleJobType);

                if (latestLogList != null && latestLogList.Any())
                {
                    var latestLog = latestLogList.First();
                    if (!schModel.PreviousTriggerTime.HasValue)
                    {
                        schModel.PreviousTriggerTime = latestLog.FireTimeUtc;
                    }
                    if (latestLog.IsSuccess.HasValue && !latestLog.IsSuccess.Value)
                    {
                        schModel.ExceptionMessage = latestLog.GetShortResultMessage();
                    }
                    else if (latestLog.IsException ?? false)
                    {
                        schModel.ExceptionMessage = latestLog.GetShortExceptionMessage();
                    }
                }
            }
        }

        private async Task NewSchedule(JobDetailModel JobDetail, TriggerDetailModel TriggerDetail)
        {
			// create schedule
			try
			{
				await SchedulerSvc.CreateSchedule(JobDetail, TriggerDetail);
			}
			catch (Exception ex)
			{
				await Snackbar.Error($"Failed to create new schedule. {ex.Message}");
				Logger.LogError(ex, "Failed to create new schedule.");
				// TODO show schedule dialog again?
			}
		}

		private async Task OnNewSchedule()
        {
            var options = new ModalInstanceOptions
            {
                Size = ModalSize.Large
            };
            await DialogSvc.Show<ScheduleDialog>("Create Schedule Job", p =>
            {
                p.Add("IsNew", true);
				//p.Add("Save", (Delegate)NewSchedule);
			}, options);
        }

        private async Task UpdateSchedule(JobDetailModel JobDetail, TriggerDetailModel TriggerDetail, Key JobKey, Key TriggerKey)
        {
			try
			{
				await SchedulerSvc.UpdateSchedule(JobKey, TriggerKey, JobDetail, TriggerDetail);
			}
			catch (Exception ex)
			{
				await Snackbar.Error($"Failed to update schedule. {ex.Message}");
				Logger.LogError(ex, "Failed to update schedule.");
				// TODO display the dialog again?
			}
		}

		private async Task OnEditScheduleJob(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                await Snackbar.Error("Cannot edit schedule. Check if job still exists.");
                return;
            }
            var currentJobDetail = await SchedulerSvc.GetJobDetail(model.JobName, model.JobGroup);

            if (currentJobDetail == null)
            {
                await Snackbar.Error("Cannot edit schedule. Check if job still exists.");
                return;
            }
            var origJobKey = new Key(currentJobDetail.Name, currentJobDetail.Group);

            TriggerDetailModel? currentTriggerModel = null;
            Key? origTriggerKey = null;
            if (model.TriggerName != null)
            {
                currentTriggerModel = await SchedulerSvc.GetTriggerDetail(model.TriggerName,
                    model?.TriggerGroup ?? Constants.DEFAULT_GROUP);

                if (currentTriggerModel != null)
                {
                    origTriggerKey = new Key(currentTriggerModel.Name, currentTriggerModel.Group);

                    ResetStartEndDateTimeIfEarlier(ref currentTriggerModel);
                }
            }

            var options = new ModalInstanceOptions
            {
                Size = ModalSize.Large
            };

            await DialogSvc.Show<ScheduleDialog>("Edit Schedule Job", p =>
            {
                p.Add("JobDetail", currentJobDetail);
                p.Add("TriggerDetail", currentTriggerModel ?? new TriggerDetailModel());
                p.Add("JobKey", origJobKey);
                p.Add("TriggerKey", origTriggerKey);
                p.Add("IsNew", false);
            }, options);
        }

        private async Task OnResumeScheduleJob(ScheduleModel model)
        {
            if (model.TriggerName == null)
            {
                await Snackbar.Error("Cannot resume schedule. Trigger name is null.");
                return;
            }

            await SchedulerSvc.ResumeTrigger(model.TriggerName, model.TriggerGroup);
        }

        private async Task OnPauseScheduleJob(ScheduleModel model)
        {
            if (model.TriggerName == null)
            {
                await Snackbar.Error("Cannot pause schedule. Trigger name is null.");
                return;
            }

            await SchedulerSvc.PauseTrigger(model.TriggerName, model.TriggerGroup);
        }

        private async Task OnDeleteScheduleJob(ScheduleModel model)
        {
            if (model.JobStatus == JobStatus.NoSchedule)
            {
                ScheduledJobs.Remove(model);
            }
            else
            {
                // confirm delete
                bool? yes = await MessageSvc.Confirm(
                    "Confirm Delete",
                    $"Do you want to delete this schedule?",
                    o =>
                    {
                        o.OkButtonText = "Yes";
                        o.CancelButtonText = "No";
                    });
                if (yes == null || !yes.Value)
                {
                    return;
                }

                var success = await SchedulerSvc.DeleteSchedule(model);

                if (!success)
                {
                    await Snackbar.Error($"Failed to delete schedule '{model.JobName}'");
                }
                else
                {
                    await Snackbar.Info("Deleted schedule");
                }
            }
        }
        private async Task OnDuplicateScheduleJob(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                await Snackbar.Error("Cannot clone schedule. Check if job still exists.");
                return;
            }
            var currentJobDetail = await SchedulerSvc.GetJobDetail(model.JobName, model.JobGroup);

            if (currentJobDetail == null)
            {
                await Snackbar.Error("Cannot clone schedule. Check if job still exists.");
                return;
            }

            TriggerDetailModel? currentTriggerModel = null;
            if (model.TriggerName != null)
            {
                currentTriggerModel = await SchedulerSvc.GetTriggerDetail(model.TriggerName,
                    model?.TriggerGroup ?? Constants.DEFAULT_GROUP);
                if (currentTriggerModel != null)
                {
                    currentTriggerModel.Name = string.Empty;
                    ResetStartEndDateTimeIfEarlier(ref currentTriggerModel);
                }
            }

            currentJobDetail.Name = string.Empty;

            var options = new ModalInstanceOptions
            {
                Size = ModalSize.Large
            };
            await DialogSvc.Show<ScheduleDialog>("Create Schedule Job", p =>
            {
                p.Add("JobDetail", currentJobDetail);
                p.Add("TriggerDetail", currentTriggerModel ?? new());
                p.Add("IsNew", true);
			}, options);
        }

        private async Task OnJobHistory(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                // not possible?
                return;
            }
            var options = new ModalInstanceOptions
            {
                Size = ModalSize.Default
            };

            await DialogSvc.Show<HistoryDialog>("Execution History", p =>
            {
                p.Add("JobKey", new Key(model.JobName, model.JobGroup));
                p.Add("TriggerKey", model.TriggerName != null ?
                    new Key(model.TriggerName, model.TriggerGroup ?? Constants.DEFAULT_GROUP) : null);
            }, options);
        }

        private async Task OnTriggerNow(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                await Snackbar.Error("Cannot add trigger. Check if job still exists.");
                return;
            }

            await SchedulerSvc.TriggerJob(model.JobName, model.JobGroup);
        }

        private async Task OnAddTrigger(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                await Snackbar.Error("Cannot add trigger. Check if job still exists.");
                return;
            }
            var currentJobDetail = await SchedulerSvc.GetJobDetail(model.JobName, model.JobGroup);

            var options = new ModalInstanceOptions
            {
                Size = ModalSize.Default
            };
            await DialogSvc.Show<ScheduleDialog>("Add New Trigger", p =>
            {
                p.Add("JobDetail", currentJobDetail);
                p.Add("IsReadOnlyJobDetail", true);
                p.Add("SelectedTab", ScheduleDialogTab.Trigger);
            }, options);
        }

        private async Task OnDeleteSelectedScheduleJobs()
        {
            if (_scheduleDataGrid is null)
                return;

            var selectedItems = _scheduleDataGrid.SelectedRows;

            if (selectedItems == null || selectedItems.Count == 0)
                return;

            // confirm delete
            bool? yes = await MessageSvc.Confirm(
                "Confirm Delete",
                $"Do you want to delete selected {selectedItems.Count} schedules?",
                o =>
                {
                    o.OkButtonText = "Yes";
                    o.CancelButtonText = "No";
                });
            if (yes == null || !yes.Value)
            {
                return;
            }

            var skipCount = 0;

            var deleteTasks = selectedItems.Select(model =>
            {
                if (model.JobStatus == JobStatus.Running)
                {
                    skipCount++;
                    return Task.FromResult(true);
                }

                ScheduledJobs.Remove(model);
                return SchedulerSvc.DeleteSchedule(model);
            });
            var results = await Task.WhenAll(deleteTasks);

            if (results == null)
            {
                await RefreshJobs();
                await Snackbar.Error("Failed to delete schedules");
            }
            else
            {
                var deletedCount = results.Count(t => t);
                var notDeletedCount = results.Count() - deletedCount - skipCount;

                if (skipCount > 0)
                {
                    await Snackbar.Info($"Deleted {deletedCount} schedule(s). Skip {skipCount} executing schedule(s)");
                }
                else
                {
                    await Snackbar.Info($"Deleted {deletedCount} schedule(s)");
                }

                if (notDeletedCount > 0)
                {
                    await RefreshJobs();
                    await Snackbar.Warning($"Failed to deleted {notDeletedCount} schedule(s)");
                }
            }
        }

        private static void ResetStartEndDateTimeIfEarlier(ref TriggerDetailModel triggerModel)
        {
            var startDateTime = triggerModel.StartDateTimeUtc;
            if (startDateTime.HasValue && startDateTime <= DateTimeOffset.UtcNow)
            {
                // clear start date if already past
                triggerModel.StartTimeSpan = null;
                triggerModel.StartDate = null;
                triggerModel.StartTimezone = TimeZoneInfo.Utc;
            }

            var endTime = triggerModel.EndDateTimeUtc;
            if (endTime.HasValue && endTime <= DateTimeOffset.UtcNow)
            {
                // clear end date if already past
                triggerModel.EndDate = null;
                triggerModel.EndTimeSpan = null;
            }
        }

        public void Dispose()
        {
            UnRegisterEventListeners();
        }

        #region Filter
        private void OnFilterClicked()
        {
            // backup original filter
            _origFilter = (ScheduleJobFilter)_filter.Clone();

            _openFilter = true;
        }

        private void OnSaveFilter()
        {
            _openFilter = false;
        }

        private async Task OnClearFilter()
        {
            _filter = new();
            await RefreshJobs();
            _openFilter = false;
        }

        private async Task OnCancelFilter()
        {
            _filter = _origFilter;
            await RefreshJobs();
            _openFilter = false;
        }

        private async Task OnIncludeSystemJobsChanged(bool value)
        {
            _filter.IncludeSystemJobs = value;
            await RefreshJobs();
        }
        #endregion Filter
    }
}

