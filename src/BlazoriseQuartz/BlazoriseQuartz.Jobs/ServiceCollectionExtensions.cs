using System;
using BlazoriseQuartz.Jobs.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazoriseQuartz.Jobs
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazoriseQuartzJobs(this IServiceCollection services)
        {
            // require to run BlazoriseQuartz.Jobs.HttpJob
            services.AddHttpClient();
            services.AddHttpClient(Constants.HttpClientIgnoreVerifySsl)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            return Abstractions.ServiceCollectionExtensions.AddBlazoriseQuartzJobs(services);
        }
    }
}

