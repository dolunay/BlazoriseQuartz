using System;
using Microsoft.EntityFrameworkCore;
using BlazoriseQuartz.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using BlazoriseQuartz.Core.History;
using BlazoriseQuartz.Core.Data;
using BlazoriseQuartz.Jobs;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace BlazoriseQuartz.Core
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBlazoriseQuartz(this IServiceCollection services,
			Action<BlazoriseQuartzCoreOptions> options,
			Action<DbContextOptionsBuilder>? dbContextOptions = null,
			string? connectionString = null)
		{
			services.Configure(options);

			BlazoriseQuartzCoreOptions coreOptions = new();
			options.Invoke(coreOptions);

			return AddBlazoriseQuartz(services, coreOptions,
				dbContextOptions, connectionString);
		}

		public static IServiceCollection AddBlazoriseQuartz(this IServiceCollection services,
			IConfiguration? config = null,
			Action<DbContextOptionsBuilder>? dbContextOptions = null,
			string? connectionString = null)
        {
			BlazoriseQuartzCoreOptions? coreOptions = null;
			if (config != null)
            {
				services.Configure<BlazoriseQuartzCoreOptions>(config);
				coreOptions = config.Get<BlazoriseQuartzCoreOptions>();
			}
			else
            {
				services.AddOptions<BlazoriseQuartzCoreOptions>()
					.Configure(opt =>
					{
					});
			}

			return AddBlazoriseQuartz(services, coreOptions ?? new(),
				dbContextOptions, connectionString);
		}

		private static IServiceCollection AddBlazoriseQuartz(IServiceCollection services,
			BlazoriseQuartzCoreOptions coreOptions,
			Action<DbContextOptionsBuilder>? dbContextOptions = null,
			string? connectionString = null)
		{
			services.AddBlazoriseQuartzJobs();

            services.TryAddSingleton<ISchedulerDefinitionService, SchedulerDefinitionService>();
			services.AddTransient<ISchedulerService, SchedulerService>();

			var schListenerSvc = new SchedulerListenerService();
			services.TryAddSingleton<ISchedulerListenerService>(schListenerSvc);
			services.AddSingleton<ITriggerListener>(schListenerSvc);
			services.AddSingleton<IJobListener>(schListenerSvc);
			services.AddSingleton<ISchedulerListener>(schListenerSvc);
			services.AddTransient<IExecutionLogStore, ExecutionLogStore>();
			services.AddTransient<IExecutionLogService, ExecutionLogService>();

			services.AddSingleton<IExecutionLogRawSqlProvider, BaseExecutionLogRawSqlProvider>();

			if (dbContextOptions != null)
            {
				services.AddDbContextFactory<BlazoriseQuartzDbContext>(dbContextOptions);
			}	
			else
			{
				Action<DbContextOptionsBuilder>? dbOptionAction = null;
				switch (coreOptions.DataStoreProvider)
				{
					case DataStoreProvider.Sqlite:
						dbOptionAction = options =>
							options.UseSqlite(connectionString ?? "DataSource=blazoriseQuartzApp.db;Cache=Shared",
								x => x.MigrationsAssembly("SqliteMigrations"))
								.UseSnakeCaseNamingConvention();
						break;
					case DataStoreProvider.InMemory:
						dbOptionAction = options =>
							options.UseInMemoryDatabase(connectionString ?? "BlazoriseQuartzDb");
						break;
					case DataStoreProvider.PostgreSQL:
						ArgumentNullException.ThrowIfNull(connectionString);
						dbOptionAction = options =>
							options.UseNpgsql(connectionString,
								x => x.MigrationsAssembly("PostgreSQLMigrations"))
								.UseSnakeCaseNamingConvention();
						break;
                    case DataStoreProvider.SqlServer:
                        ArgumentNullException.ThrowIfNull(connectionString);
                        dbOptionAction = options =>
                            options.UseSqlServer(connectionString,
                                x => x.MigrationsAssembly("SqlServerMigrations"))
                                .UseSnakeCaseNamingConvention();
                        break;
                    default:
						throw new NotSupportedException("Unsupported data store provider. Configure services.AddDbContextFactory() manually");

				}

				services.AddDbContextFactory<BlazoriseQuartzDbContext>(dbOptionAction);
			}

			services.AddHostedService<SchedulerEventLoggingService>();
			LoadJobAssemblies(coreOptions);

			return services;
		}

		private static void LoadJobAssemblies(BlazoriseQuartzCoreOptions coreOptions)
        {
			if (coreOptions.AllowedJobAssemblyFiles == null)
				return;

			var path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(SchedulerDefinitionService))!.Location) ?? String.Empty;
			List<Type> jobTypes = new();
			foreach (var assemblyStr in coreOptions.AllowedJobAssemblyFiles)
			{
				string assemblyPath = Path.Combine(path, assemblyStr + ".dll");
				Assembly.LoadFrom(assemblyPath);
			}
		}
	}
}

