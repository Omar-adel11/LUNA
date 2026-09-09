using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AuthDTOs
{
    public class ChangePasswordDTO
    {
        [Required]
        [EmailAddress]
        public string? email { get; set; } = string.Empty;
        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string password { get; set; } = string.Empty;
        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string newPassword { get; set; } = string.Empty;
        
    }
}
