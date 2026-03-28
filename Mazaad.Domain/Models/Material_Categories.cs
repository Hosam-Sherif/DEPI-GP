using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Models
{
    public class Material_Categories
    {
        public int Id { get; set; }
        public string category_name { get; set; }
        public string description { get; set; }
        public string unit_of_measure { get; set; }
        public DateTime created_at { get; set; }

        public ICollection<Listings> Listings { get; set; } = new HashSet<Listings>();
    }
}
