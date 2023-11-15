using LazyVocaApi.Entities;
using LazyVocaApi.Models;

namespace LazyVocaApi.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAsync();

        Task<User?> GetAsync(string id);

        Task<User?> GetAsync(
            string userName,
            string password);

        Task CreateAsync(User newUser);

        Task CreateAsync(string username, string password);

        Task<bool> CheckDuplicateAsync(string username);

        Task UpdateAsync(
            string id,
            User updateUser);

        Task RemoveAsync(string id);
    }
}
