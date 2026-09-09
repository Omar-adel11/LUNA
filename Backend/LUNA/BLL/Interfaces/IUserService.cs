using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.userDtos;
using DAL.Entities;
using DAL.Entities.Enums;

namespace BLL.Interfaces
{
    public interface IUserService
    {
        //GET USER by ID
        Task<UserDTO?> GetUserByIdAsync(int id);

        //UPDATE USER
        Task<UserDTO?> UpdateUserAsync(UserDTO user);

        Task<bool> ChangeLanguage(AppLanguage appLanguage);
        Task<bool> ChangeTheme(AppTheme appTheme);
        Task<bool> ChangeNotification(bool AllowNotification);

    }
}
