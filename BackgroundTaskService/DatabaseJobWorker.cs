using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomAuthenticationApp.Context;
using Microsoft.EntityFrameworkCore;

namespace BackgroundTaskService
{
    public class DatabaseJobWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseJobWorker> _logger;
        private const int PollingDelayMilliseconds = 3000;

        public DatabaseJobWorker(IServiceProvider serviceProvider, ILogger<DatabaseJobWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 MS SQL Background Processing Engine Live. Polling for Pending metrics...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessNextJobAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled execution exception caught during processing loop cycle.");
                }

                await Task.Delay(PollingDelayMilliseconds, stoppingToken);
            }
        }

        private async Task ProcessNextJobAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1. Fetch the oldest pending task assignment record
            var job = await dbContext.BackgroundJobs
                .Where(j => j.Status == "Pending")
                .OrderBy(j => j.CreatedAt)
                .FirstOrDefaultAsync(stoppingToken);

            if (job == null) return; // No work pending, exit method and wait for next tick

            // 2. Attempt to lock the job immediately by changing status to 'Processing'
            job.Status = "Processing";
            job.UpdatedAt = DateTime.UtcNow;

            try
            {
                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // 💥 Concurrency Guard: Another running worker instance claimed this row first.
                _logger.LogWarning("Job {JobId} was already claimed by another active cluster node instance. Skipping.", job.JobId);
                return;
            }

            _logger.LogInformation("⚡ Processing Job {JobId} (Type: {JobType})", job.JobId, job.JobType);

            // 3. Execute the heavy long-running calculations
            try
            {
                // ⏳ Simulate 10-second data processing, matrix audit, or reporting script execution
                await Task.Delay(10000, stoppingToken);

                job.Status = "Completed";
                job.ResultMessage = $"Process execution resolved successfully at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job execution aborted internally for identifier tokens: {JobId}", job.JobId);
                job.Status = "Failed";
                job.ResultMessage = $"Execution Failure: {ex.Message}";
            }

            // 4. Update the state record in the DB
            job.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("✔ Job {JobId} state finalized as {Status}", job.JobId, job.Status);
        }
    }
}
