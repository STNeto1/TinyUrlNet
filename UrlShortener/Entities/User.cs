using System.Text.Json.Serialization;

namespace UrlShortener.Entities;

public class User
{
    public string Id { get; private set; }
    public string Email { get; private set; }
    [JsonIgnore] public string Password { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
}