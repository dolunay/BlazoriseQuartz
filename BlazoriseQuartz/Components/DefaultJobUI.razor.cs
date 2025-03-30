using System;
using Blazorise;
using BlazoriseQuartz.Core.Models;
using BlazoriseQuartz.Models;
using Microsoft.AspNetCore.Components;

namespace BlazoriseQuartz.Components
{
	public partial class DefaultJobUI : ComponentBase
	{
		[Inject] private IModalService ModalSvc { get; set; } = null!;
		[Inject] private IMessageService DialogSvc { get; set; } = null!;

		[Parameter]
		[EditorRequired]
		public JobDetailModel JobDetail { get; set; } = new();

		[Parameter] public bool IsReadOnly { get; set; }

		public async Task AddDataMap(DataMapItemModel dataMap)
		{
			if (dataMap is { Key: not null, Value: not null })
				JobDetail.JobDataMap.Add(dataMap.Key, dataMap.Value);
			else
			{
				// TODO print error message. Data map is null
			}
			await InvokeAsync(StateHasChanged);
			await Task.CompletedTask;
		}

		private async Task OnAddDataMap()
		{
			var options = new ModalInstanceOptions
			{
				Size = ModalSize.Small
			};
			await ModalSvc.Show<JobDataMapDialog>("Add Data Map", p =>
			{
				p.Add("JobDataMap", new Dictionary<string, object>(JobDetail.JobDataMap, StringComparer.OrdinalIgnoreCase));
				p.Add("Save", AddDataMap);
			}, options);
		}

		public async Task EditDataMap(DataMapItemModel dataMap)
		{
			if (dataMap is { Key: not null, Value: not null })
			{
				JobDetail.JobDataMap[dataMap.Key] = dataMap.Value;
			}
			else
			{
				// TODO print error message. Data map is null
			}
			await InvokeAsync(StateHasChanged);
			await Task.CompletedTask;
		}

		private async Task OnEditDataMap(KeyValuePair<string, object> item)
		{
			var options = new ModalInstanceOptions
			{
				Size = ModalSize.Small
			};

			var dialog = await ModalSvc.Show<JobDataMapDialog>("Edit Data Map", p =>
			{
				p.Add("JobDataMap", JobDetail.JobDataMap);
				p.Add("DataMapItem", new DataMapItemModel(item));
				p.Add("IsEditMode", true);
				p.Add("Save", EditDataMap);

			}, options);
		}

		private async Task OnCloneDataMap(KeyValuePair<string, object> item)
		{
			var options = new ModalInstanceOptions
			{
				Size = ModalSize.Small
			};
			var index = 1;
			var key = item.Key + index++;

			while (JobDetail.JobDataMap.ContainsKey(key))
			{
				if (index == int.MaxValue)
				{
					key = string.Empty;
					break;
				}

				key = item.Key + index++;
			}
			var clonedItem = new KeyValuePair<string, object>(key, item.Value);

			await ModalSvc.Show<JobDataMapDialog>("Add Data Map", p =>
			{
				p.Add("JobDataMap", new Dictionary<string, object>(JobDetail.JobDataMap, StringComparer.OrdinalIgnoreCase));
				p.Add("DataMapItem", new DataMapItemModel(clonedItem));
				p.Add("Save", EditDataMap);
			}, options);
		}

		private async Task OnDeleteDataMap(KeyValuePair<string, object> item)
		{
			bool? yes = await DialogSvc.Confirm(
				"Confirm Delete",
				$"Do you want to delete '{item.Key}'?",
				o =>
				{
					o.OkButtonText = "Yes";
					o.CancelButtonText = "No";
				});

			if (yes == null || !yes.Value)
			{
				return;
			}

			JobDetail.JobDataMap.Remove(item);
			await InvokeAsync(StateHasChanged);
		}
	}
}

