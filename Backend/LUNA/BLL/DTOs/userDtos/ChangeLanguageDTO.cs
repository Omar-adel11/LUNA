using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.userDtos
{
    public class ChangeLanguageDTO
    {
        [Required]
        public string language { get; set; } = "en";
    }
}
