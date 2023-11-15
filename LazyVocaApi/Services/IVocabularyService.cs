using LazyVocaApi.Entities;
using LazyVocaApi.Models;
using System.Collections.Generic;

namespace LazyVocaApi.Services
{
    public interface IVocabularyService
    {
        Task<IEnumerable<Vocabulary>> GetVocabulariesAsync(string userId);

        Task<Vocabulary> GetVocabularyAsync(string id);

        Task InsertVocabulary(Vocabulary vocabulary);

        Task<Vocabulary> GetRandomWordAsync(string userId);

        Task<long> CountAsync(string userId);

        Task<AddVocabulariesResponse> CreateAsync(string rawData, string userId);

        Task UpdateEasiness(string id, int newEasiness);

        Task<VocabulariesPaging> GetVocabulariesPagingAsync(
            string userId, 
            int page = 1, 
            int pageSize = 20);
    }
}
