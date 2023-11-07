using LazyVocaApi.Models;
using LazyVocaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LazyVocaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "KMS Access")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _usersService;

        public UserController(IUserService usersService)
        {
            _usersService = usersService;   
        }

        [HttpGet]
        public async Task<List<User>> GetAllUsers()
        {
            var usrId = this.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)!.Value;

            return await _usersService.GetAsync();
        }
            
    }
}
