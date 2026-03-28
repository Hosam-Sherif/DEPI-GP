using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Models
{
    public class Notifications
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int user_id { get; set; }

        public string title { get; set; }
        public string message { get; set; }

        public bool is_read { get; set; }

        public string reference_type { get; set; }
        public int reference_id { get; set; }

        public DateTime created_at { get; set; }
        public App_Users User { get; set; }
    }
}
