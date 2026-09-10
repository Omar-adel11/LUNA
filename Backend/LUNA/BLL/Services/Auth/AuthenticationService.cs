using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.AuthDTOs;
using BLL.DTOs.userDtos;
using BLL.Exceptions.BadRequest;
using BLL.Exceptions.NotFound;
using BLL.Exceptions.Unauthorized;
using BLL.Interfaces;
using BLL.Services.Helper;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using BLL.Settings.Email;

namespace BLL.Services.Auth
{
    public class AuthenticationService(
        UserManager<User> _userManager,
        ITokenService _tokenService,
        IOTPService _oTPService,
        IEmailService _emailService,
        IHostingEnvironment _env) : IAuthenticationService
    {
        
        public async Task<UserDTO?> Login(LoginDTO loginDTO)
        {
            var user = await CheckEmailExistence(loginDTO.email);
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDTO.password);
            if (!isPasswordValid)
            {
                throw new InvalidCredentialsException();
            }
            return new UserDTO
            {
                email = user.Email,
                name = user.UserName,
                Token = await _tokenService.GenerateToken(user),
                ImgUrl = user.ImgUrl
            };
        }

        
        public async Task<UserDTO?> Signup(SignupDTO signupDTO)
        {
            var user = new User
            {
                Email = signupDTO.email,
                UserName = signupDTO.username,
            };
            var result = await _userManager.CreateAsync(user, signupDTO.password);
            if (!result.Succeeded)
            {
                throw new RegisterationBadRequestException(result.Errors.Select(e=>e.Description));
            }
            //add photo if provided
            if (signupDTO.file != null)
            {
                user.ImgUrl = DocumentSettings.UploadFile(signupDTO.file, _env.WebRootPath, "images");
                await _userManager.UpdateAsync(user);
            }
            return new UserDTO
            {
                email = user.Email,
                name = user.UserName,
                Token = await _tokenService.GenerateToken(user),
                ImgUrl = user.ImgUrl
            };
        }
        
        public async Task<string> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO,string email)
        {
            var user = await CheckEmailExistence(email);
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, changePasswordDTO.password);
            if (!isPasswordValid)
            {
                throw new InvalidOldPasswordException();
            }
            await _userManager.ChangePasswordAsync(user, changePasswordDTO.password, changePasswordDTO.newPassword);
            return "Password changed successfully";

        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await CheckEmailExistence(email);
            var otp = await _oTPService.GenerateOTP(email);
            var EmailToBeSent = new Email
            {
                To = email,
                Subject = "Password Reset OTP",
                Body = $"Your OTP is: {otp}"
            };
            await _emailService.SendEmailAsync(EmailToBeSent);
            return "OTP sent to email";
        }

        public async Task<string> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await CheckEmailExistence(resetPasswordDTO.email);
            OTPDTO oTPDTO = new OTPDTO
            {
                email = resetPasswordDTO.email,
                otp = resetPasswordDTO.otp
            };
            var ResetToken =  await _oTPService.VerifyOTP(oTPDTO);
            var result = await _userManager.ResetPasswordAsync(user, ResetToken, resetPasswordDTO.newPassword);
            if (!result.Succeeded)
            {
                throw new ResetPasswordBadRequestException(result.Errors.Select(e => e.Description));
            }
            return "Password reset successfully";
        }

        private async Task<User?> CheckEmailExistence(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new UserNotFoundException();
            }
            return user;
        }


    
       
    }
}
