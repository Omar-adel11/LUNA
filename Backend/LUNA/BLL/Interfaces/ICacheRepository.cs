using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ICacheRepository
    {
        Task<string?> GetAsync(string Key);
        Task SetAsync(string Key, object Value, TimeSpan? duration);
        Task RemoveAsync(string key);
    }
}
