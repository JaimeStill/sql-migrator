namespace App.Models;
public class ApiResult<T> : IApiResult
{
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Error { get; set; }
    public bool HasData => Data is not null;

    public ApiResult() { }

    public ApiResult(string action, Exception ex)
    {
        Error = true;
        Message = $"{typeof(T)}.{action}: {ex.Message}";
    }

    public ApiResult(T data, string message = "Operation completed successfully")
    {
        Data = data;
        Message = message;
    }

    public ApiResult(ValidationResult validation)
    {
        Error = true;
        Message = validation.Message;
    }
}