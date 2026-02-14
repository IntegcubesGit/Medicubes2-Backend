using Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Infrastructure.BackgroundServices
{
    public class FBRDataFetchBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FBRDataFetchBackgroundService> _logger;

        public FBRDataFetchBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<FBRDataFetchBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FBR Data Fetch Background Service started.");

            // Get Pakistan timezone
            TimeZoneInfo pakistanTimeZone;
            try
            {
                pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            }
            catch
            {
                // Fallback for Linux systems
                pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Get current Pakistan time
                    var pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);
                    
                    // Calculate next 00:00 Pakistan time
                    var nextRunTime = pakistanTime.Date.AddDays(1).AddHours(0).AddMinutes(0);
                    
                    // If it's before 00:00 today, schedule for today
                    if (pakistanTime.TimeOfDay < TimeSpan.FromHours(1))
                    {
                        nextRunTime = pakistanTime.Date.AddHours(0).AddMinutes(0);
                    }
                    
                    var utcNextRunTime = TimeZoneInfo.ConvertTimeToUtc(nextRunTime, pakistanTimeZone);
                    var delay = utcNextRunTime - DateTime.UtcNow;

                    if (delay.TotalMilliseconds < 0)
                    {
                        // If we somehow missed it, schedule for next day
                        nextRunTime = pakistanTime.Date.AddDays(1).AddHours(0).AddMinutes(0);
                        utcNextRunTime = TimeZoneInfo.ConvertTimeToUtc(nextRunTime, pakistanTimeZone);
                        delay = utcNextRunTime - DateTime.UtcNow;
                    }

                    _logger.LogInformation($"FBR Data Fetch scheduled for {nextRunTime:yyyy-MM-dd HH:mm:ss} Pakistan Time ({utcNextRunTime:yyyy-MM-dd HH:mm:ss} UTC). Waiting {delay.TotalHours:F2} hours.");
                    
                    // Wait until the scheduled time (or cancellation)
                    await Task.Delay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    // Execute the job
                    _logger.LogInformation("Triggering FBR Data Fetch at 00:00 Pakistan Time.");
                    
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var fbrDataFetchService = scope.ServiceProvider.GetRequiredService<IFBRDataFetchService>();
                        try
                        {
                            await fbrDataFetchService.FetchAllFBRData();
                            _logger.LogInformation("FBR Data Fetch completed successfully.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error occurred while fetching FBR data.");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in FBR Data Fetch Background Service. Retrying in 1 hour.");
                    // Wait 1 hour before retrying on error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("FBR Data Fetch Background Service stopped.");
        }
    }
}

