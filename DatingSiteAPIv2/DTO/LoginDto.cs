using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DatingSiteAPIv2.DTO
{
    public class LoginDto
    {
      
        public string Username { get; set; }
      
        public string password { get; set; }
    }
}
