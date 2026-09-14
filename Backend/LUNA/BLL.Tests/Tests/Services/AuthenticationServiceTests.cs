using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.AuthDTOs;
using BLL.DTOs.userDtos;
using BLL.Exceptions.Unauthorized;
using BLL.Interfaces;
using BLL.Services.Auth;
using DAL.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BLL.Tests.Tests.Services
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IRefreshTokenService> _mockRefreshTokenService;
        private readonly Mock<IOTPService> _mockOtpService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IHostingEnvironment> _mockEnv;

        private readonly AuthenticationService _sut;

        public AuthenticationServiceTests()
        {
            // 1. Initialize mocks for every dependency the constructor requires
            _mockUserManager = MockUserManager(); // (We'll look at this helper in a second)
            _mockTokenService = new Mock<ITokenService>();
            _mockRefreshTokenService = new Mock<IRefreshTokenService>();
            _mockOtpService = new Mock<IOTPService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockEnv = new Mock<IHostingEnvironment>();

            // 2. Instantiate the System Under Test with the fake dependencies
            _sut = new AuthenticationService(
                _mockUserManager.Object,
                _mockTokenService.Object,
                _mockRefreshTokenService.Object,
                _mockOtpService.Object,
                _mockEmailService.Object,
                _mockEnv.Object
            );
        }

        [Fact]
        public async Task logout_refreshToken()
        {
            var requestDto = new RefreshRequestDto
            {
                RefreshToken = "my-test-refresh-token"
            };
            await _sut.logout(requestDto);
            _mockRefreshTokenService.Verify(a => a.RevokeAsync("my-test-refresh-token"), Times.Once);
        }

        [Fact]
        public async Task Login_successful()
        {
            // 1. Arrange: Define the login data
            var loginDto = new LoginDTO
            {
                email = "o.adel1029@gmail.com",
                password = "myPass0rd@"
            };

            var fakeUser = new User
            {
                Id = 1,
                Email = "o.adel1029@gmail.com",
                UserName = "Omar"
            };

            // 2. Arrange: Setup the UserManager mock to return the fake user when searched by email
            _mockUserManager
                .Setup(x => x.FindByEmailAsync(loginDto.email))
                .ReturnsAsync(fakeUser);

            // 3. Arrange: Setup the UserManager mock to validate the password as true
            _mockUserManager
                .Setup(x => x.CheckPasswordAsync(fakeUser, loginDto.password))
                .ReturnsAsync(true);

            // 4. Arrange: Setup Token and Refresh Token services to return dummy strings
            _mockTokenService
                .Setup(x => x.GenerateToken(fakeUser))
                .ReturnsAsync("mock-jwt-token");

            _mockRefreshTokenService
                .Setup(x => x.GenerateAndStoreAsync(fakeUser.Id, It.IsAny<TimeSpan>()))
                .ReturnsAsync("mock-refresh-token");

            // Act
            var result = await _sut.Login(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loginDto.email, result.email);
            Assert.Equal("mock-jwt-token", result.Token);
            Assert.Equal("mock-refresh-token", result.refreshToken);

        }

        [Fact]
        public async Task Login_ThrowsException_AndNeverGeneratesToken_WhenPasswordIsInvalid()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                email = "o.adel1029@gmail.com",
                password = "WrongPassword!"
            };

            var fakeUser = new User { Id = 1, Email = "o.adel1029@gmail.com" };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.email)).ReturnsAsync(fakeUser);

            // Password check returns false (invalid password)
            _mockUserManager.Setup(x => x.CheckPasswordAsync(fakeUser, loginDto.password)).ReturnsAsync(false);

            // Act & Assert: Verify that an exception is thrown
            await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Login(loginDto));

            // Verify that GenerateToken was NEVER called because execution aborted early
            _mockTokenService.Verify(
                x => x.GenerateToken(It.IsAny<User>()),
                Times.Never
            );
        }

        private static Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }
    }
}
