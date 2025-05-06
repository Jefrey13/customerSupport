namespace CustomerService.API.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app) =>
            app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
