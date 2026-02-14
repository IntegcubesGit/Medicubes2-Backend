namespace WebApi.Extensions.ExceptionHandler
{
    public static class ExceptionHandlerExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandler>();
        }
    }
}
