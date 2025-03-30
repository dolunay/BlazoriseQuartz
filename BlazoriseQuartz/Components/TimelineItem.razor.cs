using Blazorise;
using Blazorise.Utilities;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace BlazoriseQuartz.Components
{

	/// <summary>
	/// A chronological item displayed as part of a <see cref="Timeline"/>
	/// </summary>
	/// <seealso cref="Timeline"/>
	public partial class TimelineItem : BaseComponent, IDisposable
	{
		private Color color = Color.Primary;
		private Size size = Size.Small;
		private int elevation = 1;
		private TimelineAlign timelineAlign;

		//protected string Classnames =>
		//    new CssBuilder("mud-timeline-item")
		//        .AddClass($"mud-timeline-item-{TimelineAlign.ToDescriptionString()}")
		//.AddClass(Class)
		//        .Build();

		//protected string DotClassnames =>
		//    new CssBuilder("mud-timeline-item-dot")
		//        .AddClass($"mud-timeline-dot-size-{Size.ToDescriptionString()}")
		//        .AddClass($"mud-elevation-{Elevation}")
		//.Build();

		//protected string DotInnerClassnames =>
		//    new CssBuilder("mud-timeline-item-dot-inner")
		//        .AddClass($"mud-timeline-dot-fill", Variant == Variant.Filled)
		//        .AddClass($"mud-timeline-dot-{Color.ToDescriptionString()}")
		//        .Build();

		public string DotInnerClassNames => this.DotInnerClassBuilder.Class;
		protected ClassBuilder DotInnerClassBuilder { get; private set; }

		public string DotClassNames => this.DotClassBuilder.Class;
		protected ClassBuilder DotClassBuilder { get; private set; }

		public TimelineItem() : base()
		{
			this.DotClassBuilder = new ClassBuilder(new Action<ClassBuilder>(this.BuildDotClasses));
			this.DotInnerClassBuilder = new ClassBuilder(new Action<ClassBuilder>(this.BuildDotInnerClasses));

		}
		protected virtual void BuildDotInnerClasses(ClassBuilder builder)
		{
			builder.Append("timeline-item-dot-inner");
			//builder.Append($"timeline-dot-fill", Variant == Variant.Filled);
			builder.Append($"timeline-dot-fill");
			builder.Append($"timeline-dot-{Color.Name}");
		}

		protected virtual void BuildDotClasses(ClassBuilder builder)
		{
			builder.Append("timeline-item-dot");
			builder.Append($"timeline-dot-size-{Size.ToString().ToLower()}");
			builder.Append($"elevation-{Elevation}");
		}

		protected override void BuildClasses(ClassBuilder builder)
		{
			builder.Append("timeline-item");
			builder.Append($"timeline-item-{TimelineAlign.ToString().ToLower()}");
			base.BuildClasses(builder);
		}

		/// <summary>
		/// Clears the class-names and mark them to be regenerated.
		/// </summary>
		protected virtual void DirtyMyClasses()
		{
			DirtyClasses();
			DotClassBuilder?.Dirty();
			DotInnerClassBuilder?.Dirty();
		}
		//protected override void DirtyStyles()
		//{
		//	StyleBuilder?.Dirty();
		//}

		[CascadingParameter]
		protected internal BaseItemsControl<TimelineItem>? Parent { get; set; }

		/// <summary>
		/// (Obsolete) The icon displayed for the dot.
		/// </summary>
		[Parameter]
		public string? Icon { get; set; }

		///// <summary>
		///// The display variant for the dot.
		///// </summary>
		///// <remarks>
		///// Defaults to <see cref="Variant.Outlined"/>.
		///// </remarks>
		//[Parameter]
		//public Variant Variant { get; set; } = Variant.Outlined;

		/// <summary>
		/// The CSS styles applied to the dot.
		/// </summary>
		/// <remarks>
		/// Defaults to <c>null</c>. Styles such as <c>background-color</c> can be applied (e.g. <c>background-color:red;</c>).
		/// </remarks>
		[Parameter]
		public string? DotStyle { get; set; }

		/// <summary>
		/// The color of the dot.
		/// </summary>
		/// <remarks>
		/// Defaults to <see cref="Color.Default"/>.
		/// </remarks>
		[Parameter]
		public Color Color
		{
			get => color;
			set
			{
				if (color == value)
					return;
				color = value;

				DirtyMyClasses();
			}
		}

		/// <summary>
		/// The size of the dot.
		/// </summary>
		/// <remarks>
		/// Defaults to <see cref="Size.Small"/>.
		/// </remarks>
		[Parameter]
		public Size Size
		{
			get => size; set
			{
				if (size == value)
					return;
				size = value;
				DirtyMyClasses();
			}
		}

		/// <summary>
		/// The size of the dot's drop shadow.
		/// </summary>
		/// <remarks>
		/// Defaults to <c>1</c>. A higher number creates a heavier drop shadow. Use a value of <c>0</c> for no shadow.
		/// </remarks>
		[Parameter]
		public int Elevation
		{
			get => elevation; set
			{
				if (elevation == value)
					return;
				elevation = value;
				DirtyMyClasses();
			}
		}

		/// <summary>
		/// Overrides <see cref="Timeline.TimelineAlign"/> with a custom value.
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
				DirtyMyClasses();
			}
		}

		/// <summary>
		/// Hides the dot for this item.
		/// </summary>
		/// <remarks>
		/// Defaults to <c>false</c>.
		/// </remarks>
		[Parameter]
		public bool HideDot { get; set; }

		/// <summary>
		/// The custom content for the opposite side of this item.
		/// </summary>
		/// <remarks>
		/// Defaults to <c>null</c>.
		/// </remarks>
		[Parameter]
		public RenderFragment? ItemOpposite { get; set; }

		/// <summary>
		/// The custom content for this item.
		/// </summary>
		/// <remarks>
		/// Defaults to <c>null</c>. Only applies if <see cref="ChildContent"/> is <c>null</c>.
		/// </remarks>
		[Parameter]
		public RenderFragment? ItemContent { get; set; }

		/// <summary>
		/// The custom content for the dot.
		/// </summary>
		/// <remarks>
		/// Defaults to <c>null</c>.
		/// </remarks>
		[Parameter]
		public RenderFragment? ItemDot { get; set; }

		/// <summary>
		/// The custom content for the entire item.
		/// </summary>
		/// <remarks>
		/// Defaults to <c>null</c>. When set, <see cref="ItemContent"/> will not be displayed.
		/// </remarks>
		[Parameter]
		public RenderFragment? ChildContent { get; set; }

		/// <inheritdoc />
		protected override Task OnInitializedAsync()
		{
			Parent?.Items.Add(this);

			return Task.CompletedTask;
		}

		private void Select()
		{
			var myIndex = Parent?.Items.IndexOf(this);
			Parent?.MoveTo(myIndex ?? 0);
		}

		/// <summary>
		/// Releases resources used by this component.
		/// </summary>
		public void Dispose()
		{
			Parent?.Items.Remove(this);
		}
	}
}