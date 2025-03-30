using Blazorise;
using BlazoriseQuartz.Components;
using BlazoriseQuartz.Core.Data;
using BlazoriseQuartz.Core.Data.Entities;
using BlazoriseQuartz.Core.Models;
using BlazoriseQuartz.Core.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.Text;

namespace BlazoriseQuartz.Pages.BlazoriseQuartzUI.Schedules;

public partial class HistoryDialog : ComponentBase
{
	[Inject] public IModalService ModalService { get; set; } = null!;
	[Inject] private IModalService DialogSvc { get; set; } = null!;
    [Inject] private IExecutionLogService LogSvc { get; set; } = null!;

    [EditorRequired]
    [Parameter]
    public Key JobKey { get; set; } = null!;

    [EditorRequired]
    [Parameter]
    public Key? TriggerKey { get; set; }

    private ObservableCollection<ExecutionLog> ExecutionLogs { get; } = new();
    private bool HasMore { get; set; }

    private PageMetadata? _lastPageMeta;
    private long _firstLogId;

    protected async Task Close()
    {
        await ModalService.Hide();
    }

    protected override async Task OnInitializedAsync()
    {
        await OnRefreshHistory();
    }

    private async Task GetMoreLogs()
    {
        PageMetadata pageMeta;
        if (_lastPageMeta == null)
        {
            pageMeta = new(0, 5);
        }
        else
        {
            pageMeta = _lastPageMeta with { Page = _lastPageMeta.Page + 1 };
        }

        var result = await LogSvc.GetLatestExecutionLog(JobKey.Name,
            JobKey.Group ?? Constants.DEFAULT_GROUP,
            TriggerKey?.Name, TriggerKey?.Group,
            pageMeta, _firstLogId);

        _lastPageMeta = result.PageMetadata;
        if (pageMeta.Page == 0)
        {
            _firstLogId = result.FirstOrDefault()?.LogId ?? 0;
        }

        result.ForEach(l => ExecutionLogs.Add(l));

        HasMore = result.Count == pageMeta.PageSize;
    }

    private async Task OnRefreshHistory()
    {
        ExecutionLogs.Clear();
        _lastPageMeta = null;
        _firstLogId = 0;
        HasMore = false;

        await GetMoreLogs();
    }

    private async Task OnMoreDetails(ExecutionLog log, string title)
    {
        var options = new ModalInstanceOptions
        {
            Size = ModalSize.Default
        };

        await DialogSvc.Show<ExecutionDetailsDialog>(title, p => p.Add("ExecutionLog", log), options);
    }

    private static string GetExecutionTime(ExecutionLog log)
    {
        // when fire time is available, display time range
        // otherwise just display date added
        if (log.FireTimeUtc.HasValue)
        {
            StringBuilder strBuilder = new(log.FireTimeUtc.Value.LocalDateTime.ToShortDateString() +
                    " " +
                    log.FireTimeUtc.Value.LocalDateTime.ToLongTimeString());

            var finishTime = log.GetFinishTimeUtc();
            if (finishTime.HasValue)
            {
                strBuilder.Append(" - ");
                if (finishTime.Value.LocalDateTime.Date != log.FireTimeUtc.Value.LocalDateTime.Date)
                {
                    // display ending date
                    strBuilder.Append(finishTime.Value.LocalDateTime.ToShortDateString() + " ");
                }

                strBuilder.Append(finishTime.Value.LocalDateTime.ToLongTimeString());
            }
            return strBuilder.ToString();
        }
        else
        {
            return log.DateAddedUtc.LocalDateTime.ToShortDateString() + " " +
                log.DateAddedUtc.LocalDateTime.ToLongTimeString();
        }
    }

    private static Color GetTimelineDotColor(ExecutionLog log)
    {
        return log.LogType switch
        {
            LogType.ScheduleJob => ((log.IsException ?? false) ? Color.Danger : Color.Success),
            _ => Color.Default
        };
    }
}

