namespace BatoClinic.Api.Helpers;

// Generic successful API response wrapper.
// T means this response can hold any data type:
// BranchResponseDto, DoctorResponseDto, list of appointments, etc.
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;

    public string Message { get; set; } = "Request completed successfully";

    public T? Data { get; set; }

    public ApiResponse(T? data, string? message = null)
    {
        Data = data;

        if (!string.IsNullOrWhiteSpace(message))
        {
            Message = message;
        }
    }
}