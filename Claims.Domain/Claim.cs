using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Claims.Domain
{
    /// <summary>
    /// Represents an insurance claim.
    /// </summary>
    public class Claim
    {
        [BsonId]
        public string Id { get; set; }

        [BsonElement("coverId")]
        public string CoverId { get; set; }

        [BsonElement("created")]
        public DateTime Created { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("claimType")]
        public ClaimType Type { get; set; }

        [BsonElement("damageCost")]
        public decimal DamageCost { get; set; }
    }
    /// <summary>
    /// Defines available insurance cover types.
    /// </summary>
    public enum ClaimType
    {
        Collision = 0,
        Grounding = 1,
        BadWeather = 2,
        Fire = 3
    }
}
