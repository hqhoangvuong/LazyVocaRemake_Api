using LazyVocaApi.Models;
using MongoDB.Driver.GeoJsonObjectModel;

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

        Task UpdateAsync(
            string id,
            User updateUser);

        Task RemoveAsync(string id);
    }
}
