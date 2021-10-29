using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatingSiteAPIv2.Data;
using DatingSiteAPIv2.Model;
using Microsoft.AspNetCore.Authorization;
using DatingSiteAPIv2.Interface;
using DatingSiteAPIv2.DTO;
using AutoMapper;
using System.Security.Claims;
using DatingSiteAPIv2.Extensions;
using DatingSiteAPIv2.Helpers;

namespace DatingSiteAPIv2.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        // GET: api/Users
       // [AllowAnonymous]
       //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams usersParams)
        {
            //var users = await _userRepository.GetMembersAsync(usersParams);
            //Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            //return Ok(users);

            var user = await _userRepository.GetUserByUsernameAsync(User.GetUserClaims());
            usersParams.CurrentUsername = user.UserName;

            if (string.IsNullOrEmpty(usersParams.Gender))
                usersParams.Gender = user.Gender == "male" ? "female" : "male";

            var users = await _userRepository.GetMembersAsync(usersParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);

        }

        // GET: api/Users/5
        // [Authorize]
        //[Authorize(Roles = "Member")]
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            var userToReturn = _mapper.Map<MemberDto>(user);

            if (userToReturn == null)
            {
                return NotFound();
            }

            return Ok(userToReturn);
        }

        [HttpPut]

        public async Task<ActionResult> UpdateUser( MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUserClaims();
            var user = await _userRepository.GetUserByUsernameAsync(username);

            _mapper.Map(memberUpdateDto, user);

            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update user");

        }

        [HttpPost("add-photo")]

        public async Task <ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var username = User.GetUserClaims();
            var user = await _userRepository.GetUserByUsernameAsync(username);
            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await _userRepository.SaveAllAsync())
            {
                 return CreatedAtRoute("GetUser",new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem Adding Photo");

        }


        [HttpPut("set-main-photo/{photoId}")]

        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUserClaims());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo.IsMain) return BadRequest("This Is Already Your Main Photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]

        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUserClaims());

            var photo =  user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("Main Photo Cannot Be deleted");

            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete photo");
        }


        //// PUT: api/Users/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutAppUser(int id, AppUser appUser)
        //{
        //    if (id != appUser.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(appUser).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!AppUserExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        //// POST: api/Users
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<AppUser>> PostAppUser(AppUser appUser)
        //{
        //    _context.Users.Add(appUser);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetAppUser", new { id = appUser.Id }, appUser);
        //}

        //// DELETE: api/Users/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteAppUser(int id)
        //{
        //    var appUser = await _context.Users.FindAsync(id);
        //    if (appUser == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Users.Remove(appUser);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        //private bool AppUserExists(int id)
        //{

        //    return _context.Users.Any(e => e.Id == id);
        //}
    }
}
