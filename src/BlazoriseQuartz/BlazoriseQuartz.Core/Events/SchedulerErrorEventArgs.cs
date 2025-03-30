﻿using System;
using Quartz;

namespace BlazoriseQuartz.Core.Events
{
	public class SchedulerErrorEventArgs : EventArgs
	{
		public string ErrorMessage { get; init; } = null!;
		public SchedulerException Exception { get; init; } = null!;
		public CancellationToken CancelToken { get; init; }
	}
}

