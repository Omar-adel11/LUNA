using System;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IRefreshTokenService
    {
       
        Task<string> GenerateAndStoreAsync(int userId, TimeSpan lifetime);

        
        Task<int?> ValidateAndGetUserIdAsync(string refreshToken);

        
        Task RevokeAsync(string refreshToken);
    }
}