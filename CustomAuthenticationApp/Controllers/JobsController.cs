using CustomAuthenticationApp.Context;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Entities.DTO.JobDTO;

namespace CustomAuthenticationApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "User.RunHeavyJob")]
    public class JobsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JobsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitJob([FromBody] JobRequestDto request)
        {
            var currentUserId = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var jobId = Guid.NewGuid().ToString("N");

            var newJob = new BackgroundJob
            {
                JobId = jobId,
                UserId = currentUserId,
                JobType = request.JobType,
                Status = "Pending",
                ResultMessage = "Job queued in data pool.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.BackgroundJobs.Add(newJob);
            await _context.SaveChangesAsync();

            // 🟢 HTTP 202 Accepted: Returns instantly to Blazor UI in milliseconds
            return Accepted(new JobStatusDto(jobId, "Pending", newJob.ResultMessage));
        }

        [HttpGet("status/{jobId}")]
        public async Task<IActionResult> GetJobStatus(string jobId)
        {
            var currentUserId = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var job = await _context.BackgroundJobs.AsNoTracking().FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null) return NotFound();
            if (job.UserId != currentUserId) return Forbid(); // Strict security boundary check

            return Ok(new JobStatusDto(job.JobId, job.Status, job.ResultMessage));
        }
    }
}
