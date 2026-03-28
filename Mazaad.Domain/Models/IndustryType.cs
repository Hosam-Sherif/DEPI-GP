using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Models
{
    public class IndustryType
    {
        public int Id { get; set; }
        public string IndustryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Companies> Companies { get; set; } = new HashSet<Companies>();
    }
}
