using DatingSiteAPIv2.DTO;
using DatingSiteAPIv2.Helpers;
using DatingSiteAPIv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingSiteAPIv2.Interface
{
  public  interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);
        Task<AppUser> GetUserWithLikes(int userId);
        Task<PagedList<LikeDto>> GetUserLikes(LikesParam likeParams);
    }
}
