using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PlanATech.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public UsersController(UserManager<IdentityUser> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public class ApplicationUserRegisterModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public class ApplicationUserLoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        // POST: api/Users/Register
        [HttpPost]
        [Route("Register")]
        public async Task<Object> Register(ApplicationUserRegisterModel model)
        {
            var applicationUser = new IdentityUser()
            {
                UserName = model.Email,
                Email = model.Email
            };

            try
            {
                var result = await _userManager.CreateAsync(applicationUser, model.Password);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // POST: api/Users/Login
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(ApplicationUserLoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim("Id", user.Id.ToString()));
                claims.Add(new Claim("Email", user.Email));

                var roles = await _userManager.GetRolesAsync(user);
                IdentityOptions identityOptions = new IdentityOptions();
                foreach (string role in roles)
                {
                    claims.Add(new Claim(identityOptions.ClaimsIdentity.RoleClaimType, role));
                }

                var securityTokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims.ToArray()),
                    Expires = DateTime.UtcNow.AddDays(365),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWTkey"))), SecurityAlgorithms.HmacSha256Signature)
                };

                var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
                var token = jwtSecurityTokenHandler.WriteToken(securityToken);
                return Ok(new { token });
            }
            else
                return BadRequest(new { message = "Invalid login attempt." });
        }

    }
}
