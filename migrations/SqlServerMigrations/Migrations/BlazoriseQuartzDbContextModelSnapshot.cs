﻿// <auto-generated />
using System;
using BlazoriseQuartz.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace SqlServerMigrations.Migrations
{
    [DbContext(typeof(BlazoriseQuartzDbContext))]
    partial class BlazoriseQuartzDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("BlazoriseQuartz.Core.Data.Entities.ExecutionLog", b =>
                {
                    b.Property<long>("LogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("log_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("LogId"), 1L, 1);

                    b.Property<DateTimeOffset>("DateAddedUtc")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("date_added_utc");

                    b.Property<string>("ErrorMessage")
                        .HasMaxLength(8000)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("error_message");

                    b.Property<DateTimeOffset?>("FireTimeUtc")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("fire_time_utc");

                    b.Property<bool?>("IsException")
                        .HasColumnType("bit")
                        .HasColumnName("is_exception");

                    b.Property<bool?>("IsSuccess")
                        .HasColumnType("bit")
                        .HasColumnName("is_success");

                    b.Property<bool?>("IsVetoed")
                        .HasColumnType("bit")
                        .HasColumnName("is_vetoed");

                    b.Property<string>("JobGroup")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("job_group");

                    b.Property<string>("JobName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("job_name");

                    b.Property<TimeSpan?>("JobRunTime")
                        .HasColumnType("time")
                        .HasColumnName("job_run_time");

                    b.Property<string>("LogType")
                        .IsRequired()
                        .HasColumnType("varchar(20)")
                        .HasColumnName("log_type");

                    b.Property<string>("Result")
                        .HasMaxLength(8000)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("result");

                    b.Property<int?>("RetryCount")
                        .HasColumnType("int")
                        .HasColumnName("retry_count");

                    b.Property<string>("ReturnCode")
                        .HasMaxLength(28)
                        .HasColumnType("nvarchar(28)")
                        .HasColumnName("return_code");

                    b.Property<string>("RunInstanceId")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("run_instance_id");

                    b.Property<DateTimeOffset?>("ScheduleFireTimeUtc")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("schedule_fire_time_utc");

                    b.Property<string>("TriggerGroup")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("trigger_group");

                    b.Property<string>("TriggerName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("trigger_name");

                    b.HasKey("LogId")
                        .HasName("pk_bqz_execution_logs");

                    b.HasIndex("RunInstanceId")
                        .IsUnique()
                        .HasDatabaseName("ix_bqz_execution_logs_run_instance_id")
                        .HasFilter("[run_instance_id] IS NOT NULL");

                    b.HasIndex("DateAddedUtc", "LogType")
                        .HasDatabaseName("ix_bqz_execution_logs_date_added_utc_log_type");

                    b.HasIndex("TriggerName", "TriggerGroup", "JobName", "JobGroup", "DateAddedUtc")
                        .HasDatabaseName("ix_bqz_execution_logs_trigger_name_trigger_group_job_name_job_group_date_added_utc");

                    b.ToTable("bqz_execution_logs", (string)null);
                });

            modelBuilder.Entity("BlazoriseQuartz.Core.Data.Entities.ExecutionLog", b =>
                {
                    b.OwnsOne("BlazoriseQuartz.Core.Data.Entities.ExecutionLogDetail", "ExecutionLogDetail", b1 =>
                        {
                            b1.Property<long>("LogId")
                                .HasColumnType("bigint")
                                .HasColumnName("log_id");

                            b1.Property<int?>("ErrorCode")
                                .HasColumnType("int")
                                .HasColumnName("error_code");

                            b1.Property<string>("ErrorHelpLink")
                                .HasMaxLength(1000)
                                .HasColumnType("nvarchar(1000)")
                                .HasColumnName("error_help_link");

                            b1.Property<string>("ErrorStackTrace")
                                .HasColumnType("nvarchar(max)")
                                .HasColumnName("error_stack_trace");

                            b1.Property<string>("ExecutionDetails")
                                .HasColumnType("nvarchar(max)")
                                .HasColumnName("execution_details");

                            b1.HasKey("LogId")
                                .HasName("pk_bqz_execution_log_details");

                            b1.ToTable("bqz_execution_log_details", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("LogId")
                                .HasConstraintName("fk_bqz_execution_log_details_bqz_execution_logs_log_id");
                        });

                    b.Navigation("ExecutionLogDetail");
                });
#pragma warning restore 612, 618
        }
    }
}
