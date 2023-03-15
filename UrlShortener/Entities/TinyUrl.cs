using System.Text.Json.Serialization;

namespace UrlShortener.Entities;

public class TinyUrl
{
    public int Id { get; set; }
    public string ShortUrl { get; set; }
    public string LongUrl { get; set; }

    [JsonIgnore] public int? UserId { get; set; }

    public DateTime CreatedAt { get; private set; } = DateTime.Now;
}