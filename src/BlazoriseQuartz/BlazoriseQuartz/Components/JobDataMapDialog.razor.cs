using Blazorise;
using BlazoriseQuartz.Extensions;
using BlazoriseQuartz.Models;
using Microsoft.AspNetCore.Components;

namespace BlazoriseQuartz.Components;

public partial class JobDataMapDialog : ComponentBase
{
    [Inject] private IMessageService DialogSvc { get; set; } = null!;
	[Inject] public IModalService ModalService { get; set; } = null!;

	[Parameter]
    [EditorRequired]
    public IDictionary<string, object> JobDataMap { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    [Parameter] public DataMapItemModel DataMapItem { get; set; } = new();

    [Parameter] public bool IsEditMode { get; set; }

    [Parameter] public Func<DataMapItemModel, Task>? Save { get; set; }

    private string? Value { get; set; }
    private Validations _validations = null!;

    /// <summary>
    /// DataMapType -> Description
    /// </summary>
    private IDictionary<DataMapType, string> AvailableDataMapTypes = new Dictionary<DataMapType, string>();

    protected override void OnInitialized()
    {
        // initialize available data types
        foreach(var mapType in Enum.GetValues<DataMapType>())
        {
            if (mapType == DataMapType.Object)
            {
                continue;
            }
            AvailableDataMapTypes.Add(mapType, mapType.ToString());
        }
        var currentMapType = DataMapItem.OriginalKeyValue?.GetDataMapType();
        if (currentMapType is DataMapType.Object)
        {
            AvailableDataMapTypes.Add(currentMapType.Value, 
                DataMapItem.OriginalKeyValue?.GetDataMapTypeDescription() ?? string.Empty);
        }

        Value = DataMapItem.Value?.ToString();
    }

    private void ValidateKey(ValidatorEventArgs e)
    {
        var key = Convert.ToString(e.Value);
        if (string.IsNullOrEmpty(key))
        {
            e.ErrorText = "Key is required";
            e.Status= ValidationStatus.Error;
        }

        // validate then add to dictionary
        if (!DataMapItem.IsSameKeyAsOriginal() && JobDataMap.ContainsKey(key))
        {
            e.ErrorText = "This key was already defined";
            e.Status = ValidationStatus.Error;
        }

        e.Status = ValidationStatus.Success;
    }

    private async Task OnSave()
    {
        var isValid=await _validations.ValidateAll();
        
        if (!isValid)
            return;

        try
        {
            DataMapItem.SetValue(Value);
            await Save!.Invoke(DataMapItem);
        }
        catch (Exception ex)
        {
            await DialogSvc.Error(
                "Error", 
                $"Invalid value. {ex.Message}");
            return;
        }
        
        await ModalService.Hide();
    }

    protected async Task OnCancel()
    {
		await ModalService.Hide();
	}
}