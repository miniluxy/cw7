using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using Cw5.Models;
using Cw5.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Cw5.Controllers
{
    [Route("api/enrollments")]
    [ApiController] //-> implicit model validation
    public class EnrollmentsController : ControllerBase
    {
        private IStudentsDbService _service;
        private IPasswordService _passService;
        private IConfiguration _config;

        public EnrollmentsController(IStudentsDbService service, IPasswordService passService, IConfiguration config)
        {
            _service = service;
            _passService = passService;
            _config = config;
        }


        [HttpPost]
        [Authorize]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            _service.EnrollStudent(request);
            var response = new EnrollStudentResponse();    
            if (response == null)
                return NotFound("blad 401");
            else
                //return Ok(response);
                return StatusCode(201, response);
        }

        //..

        //..
        [HttpPost("{promotions}")]
        [Authorize]
        public IActionResult PromoteStudents(PromoteStudentRequest request)
        {
            PromoteStudentResponse response = _service.PromoteStudent(request);
            if (response == null)
                return StatusCode(401, BadRequest("blad 401"));
            else
                return StatusCode(201, response);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token/{refreshToken}")]
        public IActionResult refreshToken(String refreshToken)
        {
            string login = _service.GetRefreshTokenOwner(refreshToken);
            if (login == null)
                return BadRequest("Wrong refresh token was passed");

            var claims = new[]
           {
                new Claim(ClaimTypes.Name,login),
                new Claim(ClaimTypes.Role,"Employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "Gakko",
                audience: "Employee",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials
            );
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });

        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {

            if (!_service.IsStudentExists(request.Login))
                return BadRequest("Wrong password or login");

            var requestPasswordsData = _service.getStudentPasswordData(request.Login);
            if (!_passService.Password(requestPasswordsData.Password, request.Password, requestPasswordsData.Salt))
                return BadRequest("Wrong password or login");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name,request.Login),
                new Claim(ClaimTypes.Role,"Employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "Gakko",
                audience: "Employee",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials
            );
            var TmpRefreshToken = Guid.NewGuid();
            _service.SetRefreshToken(request.Login, TmpRefreshToken.ToString());
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refershToken = TmpRefreshToken
            });
        }
    }
}