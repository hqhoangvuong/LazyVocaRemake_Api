using LazyVocaApi.Models;
using System.Collections.Generic;

namespace LazyVocaApi.Services
{
    public interface IVocabularyService
    {
        Task<IEnumerable<Vocabulary>> GetVocabulariesAsync(string userId);

        Task InsertVocabulary(Vocabulary vocabulary);

        Task<Vocabulary> GetRandomWordAsync(string userId);
    }
}
