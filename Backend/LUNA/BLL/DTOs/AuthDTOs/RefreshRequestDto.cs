using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AuthDTOs
{
    public class RefreshRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty; 
    }
}
