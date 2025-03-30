using System;
using BlazoriseQuartz.Core.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BlazoriseQuartz
{
    public static class BlazoriseQuartzBuilderExtensions
    {
        public static IApplicationBuilder UseBlazoriseQuartzUI(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var options = scope.ServiceProvider.GetRequiredService<IOptions<BlazoriseQuartzUIOptions>>().Value;
                if (options.AutoMigrateDb)
                {
                    var db = scope.ServiceProvider.GetRequiredService<BlazoriseQuartzDbContext>();
                    db.Database.Migrate();
                }
            }

            return app;
        }
    }
}

