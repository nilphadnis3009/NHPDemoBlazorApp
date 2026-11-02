using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // For [Required], [Range] etc.
using System.ComponentModel.DataAnnotations.Schema;

// 💡 ADD THIS LINE FOR THE COMPLEX TYPE VALIDATOR
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class Project : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Project name is required")]
        [StringLength(100, ErrorMessage = "Name is too long")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

       
        public ProjectBudget Budget { get; set; } = new();

        public List<TeamMember> TeamMembers { get; set; } = new();

        // 💡 Deep validation engine executes here automatically
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 1. Manually validate the nested Budget object
            var budgetResults = new List<ValidationResult>();
            var budgetContext = new ValidationContext(Budget);
            Validator.TryValidateObject(Budget, budgetContext, budgetResults, true);
            foreach (var result in budgetResults) yield return result;

            // 2. Loop and validate each member within the collection
            foreach (var member in TeamMembers)
            {
                var memberResults = new List<ValidationResult>();
                var memberContext = new ValidationContext(member);
                Validator.TryValidateObject(member, memberContext, memberResults, true);
                foreach (var result in memberResults) yield return result;
            }
        }
    }
    public enum ProjectPriority { Low, Medium, High, Critical }

    public class ProjectBudget
    {
        [Range(1000, 1000000, ErrorMessage = "Budget must be between $1,000 and $1,000,000")]
        public decimal AllocatedAmount { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public class TeamMember
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        
        [Required(ErrorMessage = "Member name is required")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
