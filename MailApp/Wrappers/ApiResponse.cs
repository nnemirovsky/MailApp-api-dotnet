namespace MailApp.Wrappers;

public class ApiResponse
{
    public bool Succeeded { get; init; } = true;
    public string? Message { get; init; }
    public object? Data { get; init; }
    
    public ApiResponse()
    { }

    public ApiResponse(object data)
    {
        Data = data;
        Succeeded = true;
    }
}
