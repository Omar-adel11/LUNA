using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Exceptions.BadRequest
{
    
    public class RegisterationBadRequestException(IEnumerable<string> Error) : BadRequestException(string.Join(",", Error))
    {
    }
}
