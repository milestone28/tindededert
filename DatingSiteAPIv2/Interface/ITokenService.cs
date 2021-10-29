using DatingSiteAPIv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingSiteAPIv2.Interface
{
   public interface ITokenService
    {
      Task  <string> CreatToken(AppUser user);
    }
}
