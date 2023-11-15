using LazyVocaApi.Entities;
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
        private readonly IVocabularyService _vocabularyService;

        public UserController(
            IUserService usersService,
            IVocabularyService vocabularyService)
        {
            _usersService = usersService;
            _vocabularyService = vocabularyService;
        }

        [HttpGet]
        [Route("/get-all")]
        public async Task<List<User>> GetAllUsers()
        {
            var usrId = this.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)!.Value;

            return await _usersService.GetAsync();
        }

        [HttpGet]
        [Route("/get-user-info")]
        public async Task<UserInfoResponse> GetUserInfo()
        {
            var usrId = this.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)!.Value;

            var user = await _usersService.GetAsync(usrId);
            var count = await _vocabularyService.CountAsync(usrId);

            return new UserInfoResponse
            {
                UserId = user!.Id,
                UserName = user.UserName,
                WordCount = count
            };
        }
    }
}
