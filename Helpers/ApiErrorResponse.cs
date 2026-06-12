namespace BatoClinic.Api.Helpers;

// Standard error response shape.
// This keeps API errors predictable for the mobile app.
public class ApiErrorResponse
{
    public bool Success { get; set; } = false;

    public string Message { get; set; } = string.Empty;

    public List<string> Errors { get; set; } = new();

    public ApiErrorResponse(string message, List<string>? errors = null)
    {
        Message = message;
        Errors = errors ?? new List<string>();
    }
}