namespace BlazoriseQuartz.Components;

/// <summary>
/// The result of a call to <see cref="Table{T}.ServerData"/>.
/// </summary>
/// <typeparam name="T">The type of item to display in the table.</typeparam>
public class TableData<T>
{
    /// <summary>
    /// The items to display in the table.
    /// </summary>
    /// <remarks>
    /// The number of items should match the number in <see cref="TableState.PageSize"/>.  
    /// </remarks>
    public IEnumerable<T>? Items { get; set; }

    /// <summary>
    /// The total number of items, excluding pagination.
    /// </summary>
    /// <remarks>
    /// This number is used to calculate the total number of pages in the table.
    /// </remarks>
    public int TotalItems { get; set; }
}