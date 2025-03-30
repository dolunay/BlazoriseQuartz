
namespace BlazoriseQuartz.Services
{
    public interface IJobUIProvider
    {
        Type GetJobUIType(string? jobTypeFullName);
    }
}