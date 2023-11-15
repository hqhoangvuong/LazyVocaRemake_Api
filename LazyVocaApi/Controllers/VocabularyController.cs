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
    public class VocabularyController : ControllerBase
    {
        private readonly IVocabularyService _vocabularyService;

        public VocabularyController(IVocabularyService vocabularyService)
        {
            _vocabularyService = vocabularyService;
        }

        [HttpPut]
        public async Task<IActionResult> InsertVocabulary([FromBody] Vocabulary vocabulary)
        {
            vocabulary.UserId = this.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)!.Value;
            vocabulary.CreatedDate = DateTime.UtcNow;
            vocabulary.UpdatedDate = DateTime.UtcNow;

            await _vocabularyService.InsertVocabulary(vocabulary);

            return NoContent();
        }

        [HttpGet("/paging/{pageIndex}/{pageSize}")]
        public async Task<IActionResult> GetVocabulariesByPaging(int pageIndex, int pageSize)
        {
            try
            {
                var usrId = this.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)!.Value;

                var result = await _vocabularyService.GetVocabulariesPagingAsync(usrId, pageIndex, pageSize);

                return Ok(result);
            } catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<Vocabulary> GetRandomWord()
        {
            var usrId = this.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)!.Value;

            return await _vocabularyService.GetRandomWordAsync(usrId);
        }

        [HttpPost]
        public async Task<AddVocabulariesResponse> InsertVocabulary([FromBody] AddVocabulariesRequest request)
        {
            var usrId = this.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)!.Value;

            var response = await _vocabularyService.CreateAsync(request.Data, usrId);

            return response;
        }

        [HttpPost("update-easiness/{wordId}/{newEasiness}")]
        public async Task<IActionResult> UpdateEasiness(string wordId, int newEasiness)
        {
            await _vocabularyService.UpdateEasiness(wordId, newEasiness);

            return NoContent();
        }
    }
}
