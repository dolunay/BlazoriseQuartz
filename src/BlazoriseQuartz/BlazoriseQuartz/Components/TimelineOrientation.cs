using System.ComponentModel;

namespace BlazoriseQuartz.Components;

/// <summary>
/// Specifies the orientation of items in a <see cref="Timeline"/>
/// </summary>
public enum TimelineOrientation
{
    /// <summary>
    /// Items are displayed vertically.
    /// </summary>
    [Description("vertical")]
    Vertical,

    /// <summary>
    /// Items are displayed horizontally.
    /// </summary>
    [Description("horizontal")]
    Horizontal
}