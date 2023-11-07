using LazyVocaApi.DatabaseSettings;
using LazyVocaApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LazyVocaApi.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserService(
            IOptions<LazyVocaDatabaseSetting> lazyVocaDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                lazyVocaDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                lazyVocaDatabaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<User>(
                lazyVocaDatabaseSettings.Value.UsersCollectionName);
        }

        public async Task<List<User>> GetAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        public async Task<User?> GetAsync(string id) =>
            await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<User?> GetAsync(
            string userName, 
            string password) =>
            await _usersCollection.Find(x => x.UserName == userName && x.Password == password).FirstOrDefaultAsync();

        public async Task CreateAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(
            string id, 
            User updateUser) => 
            await _usersCollection.ReplaceOneAsync(x => x.Id == id, updateUser);

        public async Task RemoveAsync(string id) =>
            await _usersCollection.DeleteOneAsync(x => x.Id == id);
    }
}
