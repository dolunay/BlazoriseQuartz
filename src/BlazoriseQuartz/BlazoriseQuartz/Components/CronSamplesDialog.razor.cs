using Blazorise;
using Microsoft.AspNetCore.Components;

namespace BlazoriseQuartz.Components;

public partial class CronSamplesDialog
{
	[Inject] private IModalService ModalService { get; set; } = null!;
	
	[Parameter] public Func<string, Task>? Save { get; set; }

    private readonly List<string> _cronSamples = new()
	{
		"0 15 10 ? * *",
		"0 * 14 * * ?",
		"0 0/5 10 ? * MON-FRI",
		"0 15 10 ? * 6L",
		"0 15 10 ? * 6#3",
		"0 15 10 L-2 * ?"
	};

    private string GetCronDescription(string cron)
    {
        return $"{cron} ({CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cron)})";
    }

    private async Task OnSelectExpression(string cronExpression)
    {
        await Save!.Invoke(cronExpression);
        await ModalService.Hide();
    }

    protected async Task Close()
    {
        await ModalService.Hide();
    }
}