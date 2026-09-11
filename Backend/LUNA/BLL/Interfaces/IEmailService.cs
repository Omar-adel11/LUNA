using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Settings.Email;

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(Email email);
    }
}
