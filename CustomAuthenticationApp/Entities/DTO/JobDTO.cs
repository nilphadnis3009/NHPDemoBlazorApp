using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTO
{
    public class JobDTO
    {
        public record JobRequestDto(string JobType, string ParameterPayload);
        public record JobStatusDto(string JobId, string Status, string? ResultMessage);
    }
}
