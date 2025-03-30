using Blazorise.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazoriseQuartz.Components
{
	/// <summary>
	/// Displays items in chronological order.
	/// </summary>
	/// <seealso cref="TimelineItem"/>
	public partial class Timeline : BaseItemsControl<TimelineItem>
	{
		private TimelineOrientation timelineOrientation = TimelineOrientation.Vertical;
		private TimelinePosition timelinePosition = TimelinePosition.Alternate;
		private TimelineAlign timelineAlign = TimelineAlign.Default;
		private bool reverse = false;
		private bool modifiers = true;

		protected override void BuildClasses(ClassBuilder builder)
		{
			builder.Append("timeline");
			builder.Append($"timeline-{TimelineOrientation.ToString().ToLower()}");
			builder.Append($"timeline-position-{ConvertTimelinePosition().ToString().ToLower()}");
			builder.Append($"timeline-reverse", Reverse && TimelinePosition == TimelinePosition.Alternate);
			builder.Append($"timeline-align-{TimelineAlign.ToString().ToLower()}");
			builder.Append($"timeline-modifiers", Modifiers);
			builder.Append($"timeline-rtl", RightToLeft);
			base.BuildClasses(builder);
		}

		[CascadingParameter(Name = "RightToLeft")]
		public bool RightToLeft { get; set; }

		/// <summary>
		/// The orientation of the timeline and its items.
		/// </summary>
		/// <remarks>
		/// Defaults to <see cref="TimelineOrientation.Vertical"/>.<br />
		/// When set to <see cref="TimelineOrientation.Vertical"/>, <see cref="TimelinePosition"/> can be set to <c>Left</c>, <c>Right</c>, <c>Alternate</c>, <c>Start</c>, or <c>End</c>.<br />
		/// When set to <see cref="TimelineOrientation.Horizontal"/>, <see cref="TimelinePosition"/> can be set to <c>Top</c>, <c>Bottom</c>, or <c>Alternate</c>.
		/// </remarks>
		[Parameter]
		public TimelineOrientation TimelineOrientation
		{
			get => timelineOrientation;
			set
			{
				if (timelineOrientation == value)
					return;
				timelineOrientation = value;
				DirtyClasses();
			}
		}
		/// <summary>
		/// The position the timeline and how its items are displayed.
		/// </summary>
		/// <remarks>
		/// Defaults to <see cref="TimelinePosition.Alternate"/>.<br />
		/// Can be set to <c>Left</c>, <c>Right</c>, <c>Alternate</c>, <c>Start</c>, or <c>End</c> when <see cref="TimelineOrientation"/> is <see cref="TimelineOrientation.Vertical"/>.<br />
		/// Can be set to <c>Top</c>, <c>Bottom</c>, or <c>Alternate</c> when <see cref="TimelineOrientation"/> is <see cref="TimelineOrientation.Horizontal"/>.
		/// </remarks>
		[Parameter]
		public TimelinePosition TimelinePosition
		{
			get => timelinePosition;
			set
			{
				if (timelinePosition == value)
					return;
				timelinePosition = value;
				DirtyClasses();
			}
		}

		/// <summary>
		/// The position of each item's dot relative to its text.
		/// </summary>
		/// <remarks>
		/// Defaults to <see cref="TimelineAlign.Default"/>.
		/// </remarks>
		[Parameter]
		public TimelineAlign TimelineAlign
		{
			get => timelineAlign; set
			{
				if (timelineAlign == value)
					return;
				timelineAlign = value;
				DirtyClasses();
			}
		}
		/// <summary>
		/// Reverses the order of items when <see cref="TimelinePosition"/> is <see cref="TimelinePosition.Alternate"/>.
		/// </summary>
		/// <remarks>
		/// Defaults to <c>false</c>.
		/// </remarks>
		[Parameter]
		public bool Reverse
		{
			get => reverse; set
			{
				if (reverse == value)
					return;
				reverse = value;
				DirtyClasses();
			}
		}
		/// <summary>
		/// Enables modifiers for items, such as adding a caret for a <see cref="Card"/>.
		/// </summary>
		/// <remarks>
		/// Defaults to <c>true</c>.
		/// </remarks>
		[Parameter]
		public bool Modifiers
		{
			get => modifiers; set
			{
				if (modifiers == value)
					return;
				modifiers = value;
				DirtyClasses();
			}
		}
		private TimelinePosition ConvertTimelinePosition()
		{
			if (TimelineOrientation == TimelineOrientation.Vertical)
			{
				return TimelinePosition switch
				{
					TimelinePosition.Left => RightToLeft ? TimelinePosition.End : TimelinePosition.Start,
					TimelinePosition.Right => RightToLeft ? TimelinePosition.Start : TimelinePosition.End,
					TimelinePosition.Top => TimelinePosition.Alternate,
					TimelinePosition.Bottom => TimelinePosition.Alternate,
					_ => TimelinePosition
				};
			}

			return TimelinePosition switch
			{
				TimelinePosition.Start => TimelinePosition.Alternate,
				TimelinePosition.Left => TimelinePosition.Alternate,
				TimelinePosition.Right => TimelinePosition.Alternate,
				TimelinePosition.End => TimelinePosition.Alternate,
				_ => TimelinePosition
			};
		}
	}
}