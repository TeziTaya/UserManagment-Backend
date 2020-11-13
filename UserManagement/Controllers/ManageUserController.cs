using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Dtos;
using UserManagement.Entities;
using UserManagement.Helper;
using UserManagement.Services;

namespace UserManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ManageUserController : ControllerBase
    {
        private IManageUser _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public ManageUserController(
            IManageUser userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateDto model)
        {
            try
            {
                var user = _userService.Authenticate(model.Username, model.Password);
                if (user == null)
                    return BadRequest(new { message = "Username or password is incorrect" });

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim("Id", user.UserId.ToString()),
                    new Claim("Name", user.UserName.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);
                return Ok(new
                {
                    Id = user.UserId,
                    Username = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = tokenString
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto model)
        {
            try
            {
                var user = _mapper.Map<User>(model);
                var createdUser = _userService.Create(user, model.Password);
                if (!createdUser.Success)
                {
                    return BadRequest(new
                    {
                        Errors = createdUser.Errors
                    });
                }
                var createdUserDto = _mapper.Map<UserDto>(createdUser.User);
                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
                var locationUrl = baseUrl + "/api/ManageUser/" + createdUserDto.UserId;
                return Created(locationUrl, createdUserDto);
            }
            catch(Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
            
           
        }
        [HttpGet("users")]
        public IActionResult Users()
        {
            try
            {
                var users = _userService.Users();
                var model = _mapper.Map<IList<UserDto>>(users);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            try
            {
                var user = _userService.GetUserById(id);
                var userDto = _mapper.Map<UserDto>(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _userService.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
