using System.Text.Json.Serialization;

namespace UrlShortener.Entities;

public class User
{
    public string Id { get; set; }
    public string Email { get; set; }
    [JsonIgnore] public string Password { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
}