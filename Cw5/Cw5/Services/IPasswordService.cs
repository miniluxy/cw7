using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public interface IPasswordService
    {
        String HashPassword(String password, String salt);
        String CreateSalt();
        bool Password(String hash, String password, String salt);
    }
}
