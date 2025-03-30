using Blazorise;
using BlazoriseQuartz.Core.Data.Entities;
using Microsoft.AspNetCore.Components;

namespace BlazoriseQuartz.Components;

public partial class ExecutionDetailsDialog
{
	[Inject] private IModalService ModalService { get; set; } = null!;

	[EditorRequired] [Parameter] public ExecutionLog ExecutionLog { get; set; } = new();

    protected async Task Close()
    {
        await ModalService.Hide();
    }
}