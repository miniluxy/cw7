using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.DTOs.Responses
{
    public class PasswordResponse
    {
        public String Password { get; set; }
        public String Salt { get; set; }
    }
}
