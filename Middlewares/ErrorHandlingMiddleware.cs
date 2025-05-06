using CustomerService.API.Utils;

namespace CustomerService.API.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        readonly RequestDelegate _next;
        readonly ILogger<ErrorHandlingMiddleware> _logger;
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext http)
        {
            try
            {
                await _next(http);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                http.Response.ContentType = "application/json";
                var status = ex switch
                {
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    ArgumentException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };
                http.Response.StatusCode = status;
                var response = ApiResponse<object>.Fail(
                    ex.Message,
                    status == 500 ? new[] { "Ocurrió un error interno." } : null
                );
                await http.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
