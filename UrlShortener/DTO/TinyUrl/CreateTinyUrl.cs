using System.ComponentModel.DataAnnotations;

public class CreateTinyUrl
{
    [Required, Url] public string Url { get; set; }
}