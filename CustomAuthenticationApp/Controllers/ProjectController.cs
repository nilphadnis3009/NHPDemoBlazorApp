using CustomAuthenticationApp.Context;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomAuthenticationApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<IEnumerable<Project>>> GetAll()
        {
            return await _context.Projects.Include(p=>p.TeamMembers).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetById(int id)
        {
            var project = await _context.Projects
                .Include(p => p.TeamMembers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();
            return Ok(project);
        }

        [HttpPost]
        public async Task<ActionResult> Create(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Project project)
        {
            var existing = await _context.Projects
      .Include(p => p.TeamMembers)
      .FirstOrDefaultAsync(p => p.Id == id);

            if (existing == null) return NotFound();

            // Update Root and Owned Types
            existing.Name = project.Name;
            existing.Priority = project.Priority;
            existing.Budget.AllocatedAmount = project.Budget.AllocatedAmount;
            existing.Budget.Currency = project.Budget.Currency;

            // 1. Identify which members to remove (present in DB, but missing in incoming request)
            var membersToRemove = existing.TeamMembers
                .Where(em => !project.TeamMembers.Any(pm => pm.Id == em.Id))
                .ToList();

            foreach (var member in membersToRemove)
            {
                existing.TeamMembers.Remove(member);
            }

            // 2. Identify which members to add or update
            foreach (var incomingMember in project.TeamMembers)
            {
                var currentMember = existing.TeamMembers.FirstOrDefault(em => em.Id == incomingMember.Id);

                if (currentMember != null)
                {
                    // Update existing item values (if needed, map fields here)
                    currentMember.Name = incomingMember.Name;
                    currentMember.Role = incomingMember.Role;
                }
                else
                {
                    // It's a brand new member, add it to the tracked collection
                    existing.TeamMembers.Add(incomingMember);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
