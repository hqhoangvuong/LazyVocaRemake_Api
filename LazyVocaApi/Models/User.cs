using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System.ComponentModel.DataAnnotations;

namespace LazyVocaApi.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = String.Empty;
        public string Password { get; set; } = null!;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public DateTime RegistrerDate { get; set; } = DateTime.Now;
    }

    public class UserDTO
    {
        public string Id { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        public string? Email { get; set; }
    }
}
