using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities.Enums;
using Microsoft.AspNetCore.Identity;

namespace DAL.Entities
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty;
        public string? ImgUrl { get; set; }
        public AppLanguage Language { get; set; } =AppLanguage.English;
        public bool notification { get; set; } = true;
        public AppTheme Theme { get; set; } = AppTheme.System;

        //navigation properties
        public ICollection<Session> Sessions { get; set; } = new List<Session>();

    }
}
