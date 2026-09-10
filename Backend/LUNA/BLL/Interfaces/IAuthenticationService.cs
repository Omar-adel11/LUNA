using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.AuthDTOs;
using BLL.DTOs.userDtos;
using Microsoft.AspNetCore.Identity;

namespace BLL.Interfaces
{
    public interface IAuthenticationService
    {
        //Login
        
        Task<UserDTO?> Login(LoginDTO loginDTO);

        //Signup

        Task<UserDTO?> Signup(SignupDTO signupDTO);
        
        //forget password
        Task<string> ForgotPasswordAsync(string email);

        // Verify OTP
      
        // Reset Password
       
        Task<string> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);

        // Change Password
        
        Task<string> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO,string email);
    }
}
