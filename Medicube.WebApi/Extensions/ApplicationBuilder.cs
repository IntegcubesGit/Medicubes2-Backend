namespace WebApi.Extensions
{
    public static class ApplicationBuilder
    {
        public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            if(env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseAuthentication();
            // JWT middleware Injection
            app.UseMiddleware<JwtValidationMiddleware>();

            app.UseAuthorization();

            return app;
        }
    }
}
