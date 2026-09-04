using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Session
    {
        public int Id { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //navigation properties
        public User User { get; set; } = null!;
        public int UserId { get; set; }

        public Soundscape? Soundscape { get; set; } = null!;
        public int? SoundscapeId { get; set; }

        public Intent? Intent { get; set; } = null!;
        public int? IntentId { get; set; }

    }
}
