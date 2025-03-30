using Blazorise;
using Microsoft.AspNetCore.Components;

namespace BlazoriseQuartz.Components;

public partial class EnumSwitch<T>
{
    [Parameter] public Size Size { get; set; } = Blazorise.Size.Medium;

    [Parameter] public ISet<T> ExcludedValues { get; set; } = default!;

    [Parameter] public IDictionary<T, string> ValueIcons { get; set; } = new Dictionary<T, string>();

    [Parameter] public T Value { get; set; } = default!;

    [Parameter] public EventCallback<T> ValueChanged { get; set; }
    private Type Type => typeof(T);

    private string GetIcon(int intEnum)
    {
        return ValueIcons[(T)Enum.ToObject(Type, intEnum)];
    }

    private async Task OnSelected(int value)
    {
        var preValue = Value;
        Value = (T)Enum.ToObject(Type, value);
        if (preValue.Equals(Value)) return;
        await ValueChanged.InvokeAsync(Value);
    }
}