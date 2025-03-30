using System;
using Blazorise;
using Blazorise.DataGrid;
using Blazorise.Icons.Material;
using BlazoriseQuartz.Components;
using BlazoriseQuartz.Core.Data;
using BlazoriseQuartz.Core.Data.Entities;
using BlazoriseQuartz.Core.Models;
using BlazoriseQuartz.Core.Services;
using Microsoft.AspNetCore.Components;

namespace BlazoriseQuartz.Pages.BlazoriseQuartzUI.History
{
    public partial class History : ComponentBase
    {
        [Inject] private IModalService DialogSvc { get; set; } = null!;
        [Inject] IExecutionLogService LogSvc { get; set; } = null!;

        private PagedList<ExecutionLog>? pagedData;
        private DataGrid<ExecutionLog> table = null!;

        private long _firstLogId;

        private int totalItems;
        private int pageSize = 10;
        private bool _openFilter;

        private ExecutionLogFilter _filter = new();
        private ExecutionLogFilter _origFilter = new();
        private LogType? _selectedLogType;
        private IEnumerable<string> _jobNames = Enumerable.Empty<string>();
        private IEnumerable<string> _jobGroups = Enumerable.Empty<string>();
        private IEnumerable<string> _triggerNames = Enumerable.Empty<string>();
        private IEnumerable<string> _triggerGroups = Enumerable.Empty<string>();

        async Task OnReadData()
        {
            PageMetadata pageMeta;
            var state = await table.GetState();
            if (pagedData == null)
            {
                pageMeta = new PageMetadata(0, state.PageSize);
            }
            else
            {
                pageMeta = pagedData.PageMetadata! with { Page = state.CurrentPage-1, PageSize = state.PageSize };
            }

            pagedData = await LogSvc.GetExecutionLogs(_filter, pageMeta, _firstLogId);

            if (pageMeta.Page == 0)
            {
                _firstLogId = pagedData.FirstOrDefault()?.LogId ?? 0;
            }

            ArgumentNullException.ThrowIfNull(pagedData.PageMetadata);

            totalItems = pagedData.PageMetadata.TotalCount;
        }

        private async Task OnSearch(string? text)
        {
            _filter.MessageContains = text;
            await RefreshLogs();
        }

        private async Task RefreshLogs()
        {
            pagedData = null;
            _firstLogId = 0;
            await table.ReadData.InvokeAsync();
        }

        private static (string, TextColor, string) GetLogIconAndColor(ExecutionLog log)
        {
            if (log.IsException ?? (log.IsSuccess.HasValue && !log.IsSuccess.Value))
                return (MaterialIcons.Error, TextColor.Danger, "Error");

            switch (log.LogType)
            {
                case Core.Data.LogType.ScheduleJob:
                    if (log.IsVetoed ?? false)
                        return (MaterialIcons.HighlightOff, TextColor.Warning, "Vetoed");

                    return log.IsSuccess is null ?
	                    // still running
	                    (MaterialIcons.IncompleteCircle, TextColor.Secondary, "Executing") : (MaterialIcons.Check, TextColor.Info, "Success");
                case Core.Data.LogType.Trigger:
                    return (MaterialIcons.Alarm, TextColor.Warning, "Trigger");
                default:
                    return (MaterialIcons.Info, TextColor.Warning, "System Info");
            }
        }

        private async Task OnMoreDetails(ExecutionLog log, string title)
        {
            var options = new ModalInstanceOptions
            {
                Size = ModalSize.Default
            };

            await DialogSvc.Show<ExecutionDetailsDialog>(title, p => p.Add("ExecutionLog", log), options);
        }

        #region Filters
        private async Task OnFilterClicked()
        {
            // backup original filter
            _origFilter = (ExecutionLogFilter)_filter.Clone();

            if (!_jobNames.Any())
            {
                // load filter
                await ReloadFilters();
            }

            _openFilter = true;
        }

        private void OnSaveFilter()
        {
            _openFilter = false;
        }

        private void OnClearFilter()
        {
            _filter = new();
            RefreshLogs();
            _openFilter = false;
        }

        private void OnCancelFilter()
        {
            _filter = _origFilter;
            RefreshLogs();
            _openFilter = false;
        }

        private async Task ReloadFilters()
        {
            _jobNames = await LogSvc.GetJobNames();
            _jobGroups = await LogSvc.GetJobGroups();
            _triggerNames = await LogSvc.GetTriggerNames();
            _triggerGroups = await LogSvc.GetTriggerGroups();
        }

        private void OnFilterJobGroupChanged(string? value)
        {
            _filter.JobGroup = value;
            RefreshLogs();
        }

        private void OnFilterJobNameChanged(string? value)
        {
            _filter.JobName = value;
            RefreshLogs();
        }

        private void OnFilterTriggerGroupChanged(string? value)
        {
            _filter.TriggerGroup = value;
            RefreshLogs();
        }

        private void OnFilterTriggerNameChanged(string? value)
        {
            _filter.TriggerName = value;
            RefreshLogs();
        }

        private void OnSelectedLogTypesChanged(LogType? logTypes)
        {
            _selectedLogType = logTypes;
            if (logTypes == null)
                _filter.LogTypes = null;
            else
                _filter.LogTypes = new HashSet<LogType> { logTypes.Value };

            RefreshLogs();
        }

        private void OnErrorOnlyChanged(bool errorOnly)
        {
            _filter.ErrorOnly = errorOnly;
            RefreshLogs();
        }

        private void OnIncludeSystemJobsChanged(bool flag)
        {
            _filter.IncludeSystemJobs = flag;
            RefreshLogs();
        }
        #endregion Filters
        //private Task OnPageChanged(DataGridPageChangedEventArgs args)
        //{
        //}
    }
}

