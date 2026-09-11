using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Exceptions.Unauthorized
{
    public abstract class UnauthorizedException(string message = "you are unauthorized") : Exception(message)
    {
    }
}
