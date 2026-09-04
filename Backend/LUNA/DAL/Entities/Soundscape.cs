using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Soundscape
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? SoundUrl { get; set; }
        public string Type { get; set; } = string.Empty;

        //navigation properties
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
