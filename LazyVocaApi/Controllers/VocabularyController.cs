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
    public class VocabularyController : ControllerBase
    {
        private readonly IVocabularyService _vocabularyService;

        public VocabularyController(IVocabularyService vocabularyService)
        {
            _vocabularyService = vocabularyService;
        }

        [HttpPost]
        public async Task<IActionResult> InsertVocabulary([FromBody] Vocabulary vocabulary)
        {
            vocabulary.UserId = this.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)!.Value;
            vocabulary.CreatedDate = DateTime.UtcNow;
            vocabulary.UpdatedDate = DateTime.UtcNow;

            await _vocabularyService.InsertVocabulary(vocabulary);

            return NoContent();
        }

        [HttpGet]
        public async Task<Vocabulary> GetRandomWord()
        {
            var usrId = this.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)!.Value;

            return await _vocabularyService.GetRandomWordAsync(usrId);
        }
    }
}
