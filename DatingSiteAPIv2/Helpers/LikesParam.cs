using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingSiteAPIv2.Helpers
{
    public class LikesParam : PaginationParams
    {
        public int UserId { get; set; }
        public string Predicate { get; set; }
    }
}
