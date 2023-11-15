using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;

namespace LazyVocaApi.Models
{
    public class Vocabulary
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Word { get; set; } = null!;

        public string Ipa { get; set; } = string.Empty;

        public string Meaning { get; set; }= string.Empty;

        public string Example { get; set; } = string.Empty;

        public string Translation { get; set; } = string.Empty;

        public string Synonyms { get;set; } = string.Empty;

        public string Antonyms { get; set; } = string.Empty;   

        public string Collocations { get; set; } = string.Empty;

        public string Family { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public string Skill { get; set; } = string.Empty;

        public int Complexity { get; set; } = -1;

        public int DisplayCount { get; set; } = 0;

        public string Sound { get; set; } = string.Empty;

        public int LearningStatus {  get; set; } = 0;

        public DateTime CreatedDate {  get; set; } = DateTime.MinValue;

        public DateTime UpdatedDate { get; set; } = DateTime.MinValue;

        public DateTime LastShownDate { get; set; } = DateTime.MinValue;

        public DateTime NextShownDate {  get; set; } = DateTime.MinValue;

        public DateTime LastRatedDate { get; set; } = DateTime.MinValue;
    }
}
