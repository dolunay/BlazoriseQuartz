using System;
using BlazoriseQuartz.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazoriseQuartz.Core.Test
{
    public static class CoreTestUtils
    {
        public static BlazoriseQuartzDbContext GetInMemoryBlazoriseQuartzDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<BlazoriseQuartzDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new BlazoriseQuartzDbContext(options);
        }

    }
}

