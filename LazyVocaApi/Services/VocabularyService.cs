using LazyVocaApi.DatabaseSettings;
using LazyVocaApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace LazyVocaApi.Services
{
    public class VocabularyService : IVocabularyService
    {
        private readonly IMongoCollection<Vocabulary> _vocabulariesCollection;

        public VocabularyService(
            IOptions<LazyVocaDatabaseSetting> lazyVocaDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                lazyVocaDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                lazyVocaDatabaseSettings.Value.DatabaseName);

            _vocabulariesCollection = mongoDatabase.GetCollection<Vocabulary>(
                lazyVocaDatabaseSettings.Value.VocabulariesCollectionName);
        }

        public async Task<IEnumerable<Vocabulary>> GetVocabulariesAsync(string userId)
        {
            var vocabularies = await _vocabulariesCollection.Find(x => x.UserId == userId).ToListAsync();

            return vocabularies;
        }

        public async Task InsertVocabulary(Vocabulary vocabulary)
        {
            await _vocabulariesCollection.InsertOneAsync(vocabulary);
        }

        public async Task<Vocabulary> GetRandomWordAsync(string userId)
        {
            var currentDateTime = DateTime.UtcNow.Date;

            var dateTimeMinValue = DateTime.MinValue;

            var filteredWords = await _vocabulariesCollection
                .Find(word => word.UserId == userId &&
                              (word.Complexity == -1 || word.Complexity > 0) && 
                              (word.LastShownDate != dateTimeMinValue && word.LastRatedDate != dateTimeMinValue &&
                              (word.LastShownDate < currentDateTime && word.LastShownDate > word.LastRatedDate)))
                .ToListAsync();

            if (filteredWords.Count == 0)
            {
                filteredWords = await _vocabulariesCollection
                    .Find(word => word.UserId == userId &&
                                  (word.Complexity == -1 || word.Complexity > 0) &&
                                  ((word.NextShownDate == dateTimeMinValue || word.NextShownDate == currentDateTime) ||
                                  (word.LastShownDate == dateTimeMinValue || word.LastShownDate < currentDateTime)))
                    .ToListAsync();
            }

            if (filteredWords.Count == 0)
            {
                filteredWords = await _vocabulariesCollection
                    .Find(word => word.UserId == userId &&
                                  (word.Complexity == -1 || word.Complexity > 0) &&
                                  (word.LastRatedDate == dateTimeMinValue || word.LastRatedDate == currentDateTime))
                    .ToListAsync();
            }

            var random = new Random();

            if (filteredWords.Count == 0)
            {
                // If there are no filtered words, select a random word from the entire collection
                var totalWords = await _vocabulariesCollection.CountDocumentsAsync(Builders<Vocabulary>.Filter.Empty);

                int randomIndex = random.Next(0, (int)totalWords);

                var allWords = await _vocabulariesCollection.Find(Builders<Vocabulary>.Filter.Empty).ToListAsync();

                return allWords[randomIndex];
            }

            double totalComplexity = filteredWords.Sum(word => word.Complexity == -1 ? 1 : word.Complexity);
            double totalDisplayCount = filteredWords.Sum(word => word.DisplayCount);

            var probabilities = filteredWords
                .Select(word => (word.Complexity == -1 ? 1 : word.Complexity) / totalComplexity * (1 - word.DisplayCount / totalDisplayCount))
                .ToList();

            if (probabilities.Count != filteredWords.Count)
            {
                throw new Exception(
                    $"Number of probabilities is not equal with number of filteredWords; {probabilities.Count} != {filteredWords.Count}");
            }

            double randomValue = new Random().NextDouble();
            double cumulativeProbability = 0.0;

            int selectedWordIndex = Enumerable.Range(0, filteredWords.Count)
                .FirstOrDefault(i => randomValue <= (cumulativeProbability += probabilities[i]));

            var selectedWord = filteredWords[selectedWordIndex];

            DateTime nextShownDate = selectedWord.NextShownDate == dateTimeMinValue ? currentDateTime : selectedWord.NextShownDate;
            var lastRatedDate = selectedWord.LastRatedDate == dateTimeMinValue ? currentDateTime : selectedWord.LastRatedDate;

            int interval = (int)Math.Pow(2, (currentDateTime - lastRatedDate).Days);
            nextShownDate = currentDateTime + TimeSpan.FromDays(interval);

            selectedWord.NextShownDate = nextShownDate;

            // Update the word in the database
            var filter = Builders<Vocabulary>.Filter.Eq(x => x.Id, selectedWord.Id);
            var update = Builders<Vocabulary>.Update.Set(x => x.NextShownDate, nextShownDate);

            await _vocabulariesCollection.UpdateOneAsync(filter, update);

            return selectedWord;
        }
    }
}
