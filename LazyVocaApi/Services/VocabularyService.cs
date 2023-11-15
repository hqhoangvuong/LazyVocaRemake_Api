using LazyVocaApi.DatabaseSettings;
using LazyVocaApi.Entities;
using LazyVocaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text.RegularExpressions;

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
            var newDisplayCount = selectedWord.DisplayCount + 1;

            // Update the word in the database
            var filter = Builders<Vocabulary>.Filter.Eq(x => x.Id, selectedWord.Id);
            var update = Builders<Vocabulary>.Update
                .Set(x => x.NextShownDate, nextShownDate)
                .Set(x => x.LastShownDate, DateTime.UtcNow)
                .Set(x => x.DisplayCount, newDisplayCount);

            await _vocabulariesCollection.UpdateOneAsync(filter, update);

            return selectedWord;
        }

        public async Task<long> CountAsync(string userId)
        {
            var count = await _vocabulariesCollection.CountDocumentsAsync(x => x.UserId == userId);

            return count;
        }

        public async Task<AddVocabulariesResponse> CreateAsync(string rawData, string userId)
        {
            var response = new AddVocabulariesResponse()
            {
                Updated = 0,
                Inserted = 0,
                Errors = 0
            };

            var keywordDicts = ReadByKeyword(rawData);

            foreach (var keywordDict in keywordDicts)
            {
                var word = new Vocabulary();

                foreach (var keyValuePair in keywordDict)
                {
                    switch (keyValuePair.Key.Trim().ToLower())
                    {
                        case "word":
                            word.Word = keyValuePair.Value;
                            break;
                        case "ipa":
                            word.Ipa = keyValuePair.Value;
                            break;
                        case "meaning":
                            word.Meaning = keyValuePair.Value;
                            break;
                        case "examples":
                            word.Example = keyValuePair.Value;
                            break;
                        case "synonyms":
                            word.Synonyms = keyValuePair.Value;
                            break;
                        case "antonyms":
                            word.Antonyms = keyValuePair.Value;
                            break;
                        case "collocations":
                            word.Collocations = keyValuePair.Value;
                            break;
                        case "word family":
                            word.Family = keyValuePair.Value;
                            break;
                        case "complexity":
                            Int32.TryParse(keyValuePair.Value, out int complexity);
                            word.Complexity = complexity;
                            break;
                        case "source":
                            word.Source = keyValuePair.Value;
                            break;
                        case "category":
                            word.Category = keyValuePair.Value;
                            break;
                        case "skill":
                            word.Skill = keyValuePair.Value;
                            break;
                    }
                }

                word.UserId = userId;

                if (word == null || string.IsNullOrEmpty(word.Word))
                {
                    response.Errors += 1;
                }
                else if ((await _vocabulariesCollection.CountDocumentsAsync(x => x.Word.Trim().ToLower() == word.Word.Trim().ToLower() && x.UserId == userId)) == 0)
                {
                    word.UpdatedDate = DateTime.UtcNow;
                    word.CreatedDate = DateTime.UtcNow;

                    await _vocabulariesCollection.InsertOneAsync(word);
                    response.Inserted += 1;
                }
                else
                {
                    var filter = Builders<Vocabulary>.Filter.Eq(x => x.Word, word.Word.Trim().ToLower()) &
                        Builders<Vocabulary>.Filter.Eq(x => x.UserId, word.UserId);

                    var update = Builders<Vocabulary>.Update
                        .Set(x => x.Ipa, word.Ipa)
                        .Set(x => x.Meaning, word.Meaning)
                        .Set(x => x.Example, word.Example)
                        .Set(x => x.Synonyms, word.Synonyms)
                        .Set(x => x.Antonyms, word.Antonyms)
                        .Set(x => x.Collocations, word.Collocations)
                        .Set(x => x.Family, word.Family)
                        .Set(x => x.Complexity, word.Complexity)
                        .Set(x => x.Source, word.Source)
                        .Set(x => x.Category, word.Category)
                        .Set(x => x.Skill, word.Skill);

                    await _vocabulariesCollection.UpdateOneAsync(filter, update);
                    response.Updated += 1;
                }
            }

            return response;
        }

        public async Task UpdateEasiness(string id, int newEasiness)
        {
            var filter = Builders<Vocabulary>.Filter.Eq(x => x.Id, id);
            var update = Builders<Vocabulary>.Update.Set(x => x.Complexity, newEasiness);

            await _vocabulariesCollection.UpdateOneAsync(filter, update);
        }

        private List<Dictionary<string, string>> ReadByKeyword(string userInput)
        {
            string pattern = @"(\w+):(.+?)(?=\w+:|\Z)";

            string[] rows = userInput.Split(new[] { "------" }, StringSplitOptions.None);

            List<Dictionary<string, string>> keywordDicts = new List<Dictionary<string, string>>();

            foreach (string row in rows)
            {
                MatchCollection matches = Regex.Matches(row, pattern, RegexOptions.Singleline);

                Dictionary<string, string> keywordDict = new Dictionary<string, string>();

                foreach (Match match in matches)
                {
                    string key = match.Groups[1].Value.Trim();
                    string value = match.Groups[2].Value.Trim();
                    keywordDict[key] = value;
                }

                if (keywordDict.Count > 0)
                {
                    keywordDicts.Add(keywordDict);
                }
            }

            return keywordDicts;
        }

        public Task<Vocabulary> GetVocabularyAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<VocabulariesPaging> GetVocabulariesPagingAsync(string userId, int page = 1, int pageSize = 20)
        {

            // Calculate the total number of records and pages
            long totalRecords = await _vocabulariesCollection.CountDocumentsAsync(Builders<Vocabulary>.Filter.Where(x => x.UserId == userId));
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Validate the page parameter
            if (page < 1 || page > totalPages)
            {
                throw new Exception("Invalid page number");
            }

            // Paginate the data using MongoDB's LINQ methods
            var entitiesForPage = await _vocabulariesCollection
                .Find(Builders<Vocabulary>.Filter.Empty)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new VocabulariesPaging
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                Vocabularies = entitiesForPage
            };
        }
    }
}
