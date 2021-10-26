using AutoMapper;
using DatingSiteAPIv2.Data;
using DatingSiteAPIv2.DTO;
using DatingSiteAPIv2.Helpers;
using DatingSiteAPIv2.Interface;
using DatingSiteAPIv2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DatingSiteAPIv2.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _context = context;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("register")]

        public async Task<ActionResult<UserDto>> Regiser ([FromBody] RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest(" Username Taken ");

            var user = _mapper.Map<AppUser>(registerDto);

            using var hmac = new HMACSHA512();

            //var user = new AppUser
            //{
            //    UserName = registerDto.Username.ToLower(),
            //    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            //    PasswordSalt = hmac.Key
            //};


            user.UserName = registerDto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;
           

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new UserDto
            { 
                Username = user.UserName,
                Token = _tokenService.CreatToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender,
          
            };
        }
         
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto loginDto)
        {
            //username
            var user = await _context.Users.Include(x => x.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

            if (user == null)
                return Unauthorized("Invalid Username");

            //password
            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.password));

            for(int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                    return Unauthorized("Invalid Password");
            }

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreatToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                 Gender = user.Gender,
            };
        }

 
        // condition

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(e => e.UserName == username.ToLower());
        }

    }

}
