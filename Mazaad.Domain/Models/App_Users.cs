using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Models
{
    public class App_Users
    {
        public int Id { get; set; }

        [ForeignKey("Company")]
        public int company_id { get; set; }

        public string full_name { get; set; }
        public string email { get; set; }
        public string password_hash { get; set; }
        public string job_title { get; set; }
        public bool is_active { get; set; }
        public DateTime last_login_date { get; set; }
        public DateTime created_at { get; set; }

        public Companies Company { get; set; }
        public ICollection<Messages> Messages { get; set; } = new HashSet<Messages>();
        public ICollection<Bids> Bids { get; set; } = new HashSet<Bids>();
        public ICollection<Notifications> Notifications { get; set; } = new HashSet<Notifications>();
    }
}
