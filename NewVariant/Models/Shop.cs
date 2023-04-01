using System.Text.Json.Serialization;
using NewVariant.Interfaces;

namespace NewVariant.Models {
    public record Shop : IEntity {
        [JsonPropertyName("id")] public int Id { get; }
        [JsonPropertyName("name")] public string Name { get; }
        [JsonPropertyName("city")] public string City { get; }
        [JsonPropertyName("country")] public string Country { get; }
        
        public Shop(string name, string city, string country) {
            Id = _entityCounter++;
            Name = name;
            City = city;
            Country = country;
        }

        [JsonConstructor]
        public Shop(int id, string name, string city, string country) {
            Id = id;
            Name = name;
            City = city;
            Country = country;
        }
        
        private static int _entityCounter;
    }
}