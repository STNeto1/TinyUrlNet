using System.Text.Json.Serialization;

namespace UrlShortener.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    [JsonIgnore] public string Password { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now;

    [JsonIgnore] public List<TinyUrl> Urls { get; set; }
}