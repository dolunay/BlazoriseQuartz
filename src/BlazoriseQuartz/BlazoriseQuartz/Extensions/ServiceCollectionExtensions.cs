using System;
using Microsoft.Extensions.DependencyInjection;
using BlazoriseQuartz.Core;
using BlazoriseQuartz.Services;
using BlazoriseQuartz.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Blazorise;
using Blazorise.Icons.Material;
using Blazorise.Material;

namespace BlazoriseQuartz
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazoriseQuartzUI(this IServiceCollection services,
            IConfiguration blazoriseUIConfiguration,
            Action<DbContextOptionsBuilder>? dbContextOptions = null,
            string? connectionString = null)
        {
            services.Configure<BlazoriseQuartzUIOptions>(blazoriseUIConfiguration);

            var uiOptions = blazoriseUIConfiguration.Get<BlazoriseQuartzUIOptions>();
            services.AddBlazoriseQuartz(blazoriseUIConfiguration, dbContextOptions, connectionString);

            return AddBlazoriseQuartzUI(services);
        }

        public static IServiceCollection AddBlazoriseQuartzUI(this IServiceCollection services,
            Action<BlazoriseQuartzUIOptions>? configure = null,
            Action<DbContextOptionsBuilder>? dbContextOptions = null,
            string? connectionString = null)
        {
            if (configure == null)
            {
                services.AddOptions<BlazoriseQuartzUIOptions>()
                    .Configure(opt =>
                    {
                    });
                services.AddBlazoriseQuartz(dbContextOptions: dbContextOptions,
                    connectionString: connectionString);
            }
            else
            {
                BlazoriseQuartzUIOptions uiOptions = new();
                services.Configure(configure);
                services.AddBlazoriseQuartz(
                    o =>
                    {
                        o.AllowedJobAssemblyFiles = uiOptions.AllowedJobAssemblyFiles;
                        o.AutoMigrateDb = uiOptions.AutoMigrateDb;
                        o.DataStoreProvider = uiOptions.DataStoreProvider;
                        o.DisallowedJobTypes = uiOptions.DisallowedJobTypes;
                    },
                    dbContextOptions,
                    connectionString);
            }

            return AddBlazoriseQuartzUI(services);
        }

        private static IServiceCollection AddBlazoriseQuartzUI(IServiceCollection services)
        {
            // Blazorise
            services
	            .AddBlazorise(options =>
	            {
		            options.Immediate = true;
	            })
	            .AddMaterialProviders()
	            .AddMaterialIcons()             
	            //.AddBootstrap5Providers()
	            //.AddFontAwesomeIcons()
                ;

            services.AddScoped<LayoutService>();
            services.AddTransient<ITriggerDetailModelValidator, TriggerDetailModelValidator>();
            services.AddSingleton<IJobUIProvider, JobUIProvider>();

            return services;
        }

    }
}

