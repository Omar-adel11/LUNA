using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.AuthDTOs;
using BLL.Interfaces;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController(IServiceManager serviceManager,SignInManager<User> signInManager) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> login(LoginDTO loginDTO)
        {
            var result =  await serviceManager.AuthService.Login(loginDTO);
            return Ok(result);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> signUp([FromForm]SignupDTO signupDTO)
        {
            var result = await serviceManager.AuthService.Signup(signupDTO);
            return Ok(result);
        }
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> changePassword(ChangePasswordDTO changePasswordDTO)
        {
            var email = User.FindFirst(ClaimTypes.Email).Value;
            var result = await serviceManager.AuthService.ChangePasswordAsync(changePasswordDTO, email);
            return Ok(new { message = result });
        }
        
        [HttpPost("forget-password")]
        public async Task<IActionResult> forgetPassword([FromBody]string email)
        {
            var result = await serviceManager.AuthService.ForgotPasswordAsync(email);
            return Ok(result);

        }

        [HttpPost("check-otp")]
        public async Task<IActionResult> CheckOtp([FromBody] CheckOtpDTO checkOtpDTO)
        {
            var result = await serviceManager.AuthService.CheckOtpAsync(checkOtpDTO);
            return Ok(result);

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> resetPassword(ResetPassDto resetPasswordDTO)
        {
            var result = await serviceManager.AuthService.ResetPasswordAsync(resetPasswordDTO);
            return Ok(result);
        }

    }
}
