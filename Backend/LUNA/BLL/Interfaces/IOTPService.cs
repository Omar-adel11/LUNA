using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.AuthDTOs;

namespace BLL.Interfaces
{
    public interface IOTPService
    {
        Task<string> GenerateOTP(string email);
        Task<string> VerifyOTP(OTPDTO oTPDTO);
    }
}
