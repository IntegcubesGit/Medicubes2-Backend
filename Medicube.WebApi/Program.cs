using Application.Common.Interfaces;
using Infrastructure;
using Infrastructure.BackgroundServices;
using Infrastructure.Repositories;
using Infrastructure.Services;
using System.Net;
using WebApi.Extensions;
using WebApi.Extensions.ExceptionHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddHttpClient<IFBRService, FBRService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = true,
    CookieContainer = new CookieContainer(),
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
});

builder.Services.AddHttpClient<IFBRDataFetchService, FBRDataFetchService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(10); // Longer timeout for bulk operations
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = true,
    CookieContainer = new CookieContainer(),
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
});

builder.Services.AddSwaggerWithJwt();

// Register Background Service for FBR Data Fetch
builder.Services.AddHostedService<FBRDataFetchBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseCustomMiddlewares(app.Environment);

app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
