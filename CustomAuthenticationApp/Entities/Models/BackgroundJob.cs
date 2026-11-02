
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace Entities.Models
{
    public class BackgroundJob
    {
        [Key]
        [StringLength(36)]
        public string JobId { get; set; } = null!;

        [StringLength(50)]
        public string UserId { get; set; } = null!;

        [StringLength(50)]
        public string JobType { get; set; } = null!;

        [StringLength(20)]
        public string Status { get; set; } = null!;

        public string? ResultMessage { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!; // Maps directly to MSSQL ROWVERSION
    }
}