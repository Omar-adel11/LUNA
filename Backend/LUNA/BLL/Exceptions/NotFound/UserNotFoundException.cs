using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Exceptions.NotFound
{
    public class UserNotFoundException() : NotFoundException("user not found")
    {
    }
}
