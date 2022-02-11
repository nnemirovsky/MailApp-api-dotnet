namespace MailApp.Models;

public class Letter
{
    public int Id { get; init; }
    public string Subject { get; set; }
    public DateTime CreatedAt { get; set; }
    public string[] Recipients { get; set; }
    public string Sender { get; set; }
    public string[] Tags { get; set; }
    public string Body { get; set; }
}

public class IndexLetterDto
{
    public string Subject { get; set; }
    public DateTime CreatedAt { get; set; }
    public string[] Recipients { get; set; }
    public string Sender { get; set; }
    public string[] Tags { get; set; }
    public string Body { get; set; }
}

public class CreateLetterDto
{
    public string Subject { get; set; }
    public string[] Recipients { get; set; }
    public string Sender { get; set; }
    public string[] Tags { get; set; }
    public string Body { get; set; }
}

public class UpdateLetterDto
{
    public string? Subject { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string[]? Recipients { get; set; }
    public string? Sender { get; set; }
    public string[]? Tags { get; set; }
    public string? Body { get; set; }
}
