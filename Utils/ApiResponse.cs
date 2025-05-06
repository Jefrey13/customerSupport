namespace CustomerService.API.Utils
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; }
        public T? Data { get; init; }
        public IEnumerable<string>? Errors { get; init; }

        public ApiResponse(T? data, string message = "", bool success = true, IEnumerable<string>? errors = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors;
        }

        public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) =>
            new ApiResponse<T>(default, message, false, errors);
    }
}
