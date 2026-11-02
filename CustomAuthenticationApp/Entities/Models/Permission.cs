using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class Permission
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty; // e.g., "User.Read", "Invoice.Delete"
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
