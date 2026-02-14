using Application.Common.Interfaces;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories
{
    public class FBRDataFetchService : IFBRDataFetchService
    {
        private readonly AppDbContext _db;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://gw.fbr.gov.pk/pdi/v1";
        private const string BaseUrlV2 = "https://gw.fbr.gov.pk/pdi/v2";

        public FBRDataFetchService(AppDbContext db, HttpClient httpClient)
        {
            _db = db;
            _httpClient = httpClient;
        }

        public async Task<object> FetchAllFBRData()
        {
            var results = new List<object>();

            // Get all org app settings with FBR sandbox tokens
            var orgSettings = await _db.OrgAppSettings
                .Where(a => a.IsDeleted == 0 && !string.IsNullOrWhiteSpace(a.FbrSandboxToken))
                .ToListAsync();

            if (!orgSettings.Any())
            {
                return new { message = "No organization settings with FBR tokens found", success = false };
            }

            foreach (var orgSetting in orgSettings)
            {
                var log = new FBRDataFetchLog
                {
                    TriggerAt = DateTime.UtcNow,
                    UserId = orgSetting.CreatedBy
                };

                try
                {
                    // 1. Fetch HSCode
                    await FetchHSCode(orgSetting.FbrSandboxToken!, log);

                    // 2. Fetch States
                    await FetchStates(orgSetting.FbrSandboxToken!, log);

                    // 3. Fetch Invoice Types
                    await FetchInvoiceTypes(orgSetting.FbrSandboxToken!, log);

                    // 4. Fetch Sale Types
                    await FetchSaleTypes(orgSetting.FbrSandboxToken!, log);

                    // 5. Fetch Unit Types (UoM)
                    await FetchUnitTypes(orgSetting.FbrSandboxToken!, log);

                    // 6. Fetch Tax Rates (requires states and sale types - nested loop)
                    await FetchTaxRates(orgSetting.FbrSandboxToken!, log);

                    // 7. Fetch SRO Schedules (requires tax rates and states - nested loop)
                    await FetchSroSchedules(orgSetting.FbrSandboxToken!, log);

                    // 8. Fetch SRO Items (requires SRO schedules)
                    await FetchSroItems(orgSetting.FbrSandboxToken!, log);
                }
                catch (Exception ex)
                {
                    // Log general error if any step fails catastrophically
                    var errorMsg = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMsg += $" Inner Exception: {ex.InnerException.Message}";
                    }
                    log.IhsCodeError = log.IhsCodeError ?? errorMsg;
                }

                try
                {
                    await _db.FBRDataFetchLogs.AddAsync(log);
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // If we can't save the log, include it in the error
                    var errorMsg = $"Failed to save FBR data fetch log. Error: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMsg += $" Inner Exception: {ex.InnerException.Message}";
                    }
                    throw new Exception(errorMsg, ex);
                }

                results.Add(new
                {
                    SettingId = orgSetting.SettingId,
                    UserId = log.UserId,
                    LogId = log.Id,
                    Status = "Completed",
                    Log = log
                });
            }

            return new { success = true, results = results };
        }

        private async Task FetchHSCode(string token, FBRDataFetchLog log)
        {
            try
            {
                var url = $"{BaseUrl}/itemdesccode";
                var response = await MakeApiCall<List<FBRHSCodeResponse>>(url, token);

                if (response != null && response.Any())
                {
                    var existingCodes = await _db.IHSCodes
                        .Where(x => x.IsDeleted == 0)
                        .Select(x => x.Code)
                        .ToListAsync();

                    var newItems = response
                        .Where(x => !string.IsNullOrWhiteSpace(x.HS_CODE) && !existingCodes.Contains(x.HS_CODE))
                        .Select(x => new IHSCode
                        {
                            Code = x.HS_CODE!,
                            Description = x.Description,
                            IsDeleted = 0
                        })
                        .ToList();

                    if (newItems.Any())
                    {
                        await _db.IHSCodes.AddRangeAsync(newItems);
                        await _db.SaveChangesAsync();
                        log.IhsCodeCount = newItems.Count;
                    }
                    else
                    {
                        log.IhsCodeCount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                log.IhsCodeCount = -1;
                log.IhsCodeError = ex.Message; // Already includes endpoint URL from MakeApiCall
            }
        }

        private async Task FetchStates(string token, FBRDataFetchLog log)
        {
            try
            {
                var url = $"{BaseUrl}/provinces";
                var response = await MakeApiCall<List<FBRStateResponse>>(url, token);

                if (response != null && response.Any())
                {
                    var existingStates = await _db.IStateTypes
                        .Where(x => x.IsDeleted == 0)
                        .Select(x => x.StateId)
                        .ToListAsync();

                    var newItems = response
                        .Where(x => !existingStates.Contains(x.StateProvinceCode) && !string.IsNullOrWhiteSpace(x.StateProvinceDesc))
                        .Select(x => new IStateType
                        {
                            StateId = x.StateProvinceCode,
                            StateName = x.StateProvinceDesc!,
                            IsDeleted = 0
                        })
                        .ToList();

                    if (newItems.Any())
                    {
                        await _db.IStateTypes.AddRangeAsync(newItems);
                        await _db.SaveChangesAsync();
                        log.IStateCount = newItems.Count;
                    }
                    else
                    {
                        log.IStateCount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                log.IStateCount = -1;
                log.IStateError = ex.Message; // Already includes endpoint URL from MakeApiCall
            }
        }

        private async Task FetchInvoiceTypes(string token, FBRDataFetchLog log)
        {
            try
            {
                var url = $"{BaseUrl}/doctypecode";
                var response = await MakeApiCall<List<FBRInvoiceTypeResponse>>(url, token);

                if (response != null && response.Any())
                {
                    var existingTypes = await _db.IInvoiceTypes
                        .Select(x => x.InvoiceTypeId)
                        .ToListAsync();

                    var newItems = response
                        .Where(x => !existingTypes.Contains(x.DocTypeId) && !string.IsNullOrWhiteSpace(x.DocDescription))
                        .Select(x => new IInvoiceType
                        {
                            InvoiceTypeId = x.DocTypeId,
                            InvoiceTypeDescription = x.DocDescription!
                        })
                        .ToList();

                    if (newItems.Any())
                    {
                        await _db.IInvoiceTypes.AddRangeAsync(newItems);
                        await _db.SaveChangesAsync();
                        log.IInvoiceTypeCount = newItems.Count;
                    }
                    else
                    {
                        log.IInvoiceTypeCount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                log.IInvoiceTypeCount = -1;
                log.IInvoiceTypeError = ex.Message; // Already includes endpoint URL from MakeApiCall
            }
        }

        private async Task FetchSaleTypes(string token, FBRDataFetchLog log)
        {
            try
            {
                var url = $"{BaseUrl}/transtypecode";
                var response = await MakeApiCall<List<FBRSaleTypeResponse>>(url, token);

                if (response != null && response.Any())
                {
                    var existingTypes = await _db.ISaleTypes
                        .Where(x => x.IsDeleted == 0)
                        .Select(x => x.SaleTypeId)
                        .ToListAsync();

                    var newItems = response
                        .Where(x => !existingTypes.Contains(x.TRANSACTION_TYPE_ID) && !string.IsNullOrWhiteSpace(x.TRANSACTION_DESC))
                        .Select(x => new ISaleType
                        {
                            SaleTypeId = x.TRANSACTION_TYPE_ID,
                            SaleName = x.TRANSACTION_DESC!,
                            IsDeleted = 0
                        })
                        .ToList();

                    if (newItems.Any())
                    {
                        await _db.ISaleTypes.AddRangeAsync(newItems);
                        await _db.SaveChangesAsync();
                        log.ISaleTypeCount = newItems.Count;
                    }
                    else
                    {
                        log.ISaleTypeCount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ISaleTypeCount = -1;
                log.ISaleTypeError = ex.Message; // Already includes endpoint URL from MakeApiCall
            }
        }

        private async Task FetchUnitTypes(string token, FBRDataFetchLog log)
        {
            try
            {
                var url = $"{BaseUrl}/uom";
                var response = await MakeApiCall<List<FBRUoMResponse>>(url, token);

                if (response != null && response.Any())
                {
                    var existingUnits = await _db.IUnitTypes
                        .Where(x => x.IsDeleted == 0)
                        .Select(x => x.UnitId)
                        .ToListAsync();

                    var newItems = response
                        .Where(x => !existingUnits.Contains(x.UOM_ID) && !string.IsNullOrWhiteSpace(x.Description))
                        .Select(x => new IUnitType
                        {
                            UnitId = x.UOM_ID,
                            UnitName = x.Description!,
                            IsDeleted = 0
                        })
                        .ToList();

                    if (newItems.Any())
                    {
                        await _db.IUnitTypes.AddRangeAsync(newItems);
                        await _db.SaveChangesAsync();
                        log.IUnitTypeCount = newItems.Count;
                    }
                    else
                    {
                        log.IUnitTypeCount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                log.IUnitTypeCount = -1;
                log.IUnitTypeError = ex.Message; // Already includes endpoint URL from MakeApiCall
            }
        }

        private async Task FetchTaxRates(string token, FBRDataFetchLog log)
        {
            try
            {
                var saleTypes = await _db.ISaleTypes.Where(x => x.IsDeleted == 0).ToListAsync();
                var states = await _db.IStateTypes.Where(x => x.IsDeleted == 0).ToListAsync();
                // Format: "24-Feb-2024" -> "dd-MMM-yyyy"
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
                var pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);
                // Format: "14-jan-2025" -> lowercase month abbreviation
                var currentDate = pakistanTime.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture).ToLowerInvariant();

                int totalInserted = 0;
                int totalErrors = 0;
                var errorMessages = new List<string>();
                // Check uniqueness based on TaxRateId + StateId + SaleTypeId combination
                var existingRates = await _db.ITaxRates
                    .Where(x => x.IsDeleted == 0)
                    .Select(x => new { x.TaxRateId, x.StateId, x.SaleTypeId })
                    .ToListAsync();

                // Get the maximum RateId to generate unique values for new records
                // RateId is only used as PRIMARY KEY, not for business logic
                // Query all records (including deleted) to avoid primary key conflicts
                int maxRateId = await _db.ITaxRates.AnyAsync() 
                    ? await _db.ITaxRates.MaxAsync(x => (int?)x.RateId) ?? 0
                    : 0;
                int nextRateId = maxRateId + 1;

                foreach (var saleType in saleTypes)
                {
                    foreach (var state in states)
                    {
                        try
                        {
                            var url = $"{BaseUrlV2}/SaleTypeToRate?date={currentDate}&transTypeId={saleType.SaleTypeId}&originationSupplier={state.StateId}";
                            // API returns an array of tax rates
                            var responses = await MakeApiCall<List<FBRTaxRateResponse>>(url, token);

                            if (responses != null && responses.Any())
                            {
                                foreach (var response in responses)
                                {
                                    if (response != null && response.RateId.HasValue && response.RateValue.HasValue && !string.IsNullOrWhiteSpace(response.RateTitle))
                                    {
                                        // Check if TaxRateId + StateId + SaleTypeId combination already exists
                                        // A record is unique based on TaxRateId + StateId + SaleTypeId combination
                                        var exists = existingRates.Any(x => 
                                            x.TaxRateId == response.RateId.Value && 
                                            x.StateId == state.StateId &&
                                            x.SaleTypeId == saleType.SaleTypeId);
                                        
                                        if (!exists)
                                        {
                                            try
                                            {
                                                var taxRate = new ITaxRate
                                                {
                                                    RateId = nextRateId++, // Generate unique RateId (only used as PRIMARY KEY)
                                                    TaxRateId = response.RateId.Value, // Store API's ratE_ID in TaxRateId
                                                    RateTitle = response.RateTitle!,
                                                    RateValue = response.RateValue.Value,
                                                    SaleTypeId = saleType.SaleTypeId, // Store ISaleType.SaleTypeId
                                                    StateId = state.StateId,
                                                    IsDeleted = (short)0
                                                };

                                                await _db.ITaxRates.AddAsync(taxRate);
                                                await _db.SaveChangesAsync();
                                                totalInserted++;

                                                // Update the existingRates list to include the new record
                                                existingRates.Add(new { taxRate.TaxRateId, taxRate.StateId, taxRate.SaleTypeId });
                                            }
                                            catch (Exception saveEx)
                                            {
                                                // Log database save errors
                                                totalErrors++;
                                                var saveErrorMsg = $"Failed to save TaxRate: SaleTypeId={saleType.SaleTypeId}, StateId={state.StateId}, TaxRateId={response.RateId.Value}. Error: {saveEx.Message}";
                                                errorMessages.Add(saveErrorMsg);
                                                // Continue processing other records
                                            }
                                        }
                                    }
                                }
                            }
                            // If responses is null or empty, continue to next combination (no error, just no data)
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue processing other combinations
                            totalErrors++;
                            var errorMsg = $"SaleTypeId={saleType.SaleTypeId}, StateId={state.StateId}: {ex.Message}";
                            errorMessages.Add(errorMsg);
                            // Continue processing other combinations instead of returning
                        }
                    }
                }

                // Set error message if there were any errors
                if (totalErrors > 0)
                {
                    log.ITaxRateError = $"Completed with {totalErrors} error(s). Successfully inserted: {totalInserted}. Errors: {string.Join("; ", errorMessages.Take(5))}";
                    if (errorMessages.Count > 5)
                    {
                        log.ITaxRateError += $" (and {errorMessages.Count - 5} more)";
                    }
                }

                log.ITaxRateCount = totalInserted;
            }
            catch (Exception ex)
            {
                log.ITaxRateCount = -1;
                log.ITaxRateError = ex.Message; // Already includes endpoint URL from MakeApiCall
            }
        }

        private async Task FetchSroSchedules(string token, FBRDataFetchLog log)
        {
            try
            {
                var taxRates = await _db.ITaxRates.Where(x => x.IsDeleted == 0).ToListAsync();
                var states = await _db.IStateTypes.Where(x => x.IsDeleted == 0).ToListAsync();
                // Format: "04-Feb-2024" -> "dd-MMM-yyyy"
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
                var pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);
                var currentDate = pakistanTime.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                int totalInserted = 0;
                var existingSchedules = await _db.ISroSchedules
                    .Where(x => x.IsDeleted == 0)
                    .Select(x => new { x.RateId, x.StateId, x.SerNo, x.Title })
                    .ToListAsync();

                foreach (var rate in taxRates)
                {
                    foreach (var state in states)
                    {
                        try
                        {
                            // Use TaxRateId (from API's ratE_ID) for the API call
                            // rate.RateId is the PK, rate.TaxRateId is the business key from API
                            var url = $"{BaseUrl}/SroSchedule?rate_id={rate.TaxRateId}&date={currentDate}&origination_supplier_csv={state.StateId}";
                            var response = await MakeApiCall<List<FBRSroScheduleResponse>>(url, token);

                            if (response != null && response.Any())
                            {
                                foreach (var sro in response)
                                {
                                    // Check if required fields are present
                                    // SerNo can be 0, so we check HasValue to ensure it's not null
                                    if (sro.SroId.HasValue && sro.SerNo.HasValue && !string.IsNullOrWhiteSpace(sro.Title))
                                    {
                                        // Check uniqueness by RateId (PK) + StateId + SerNo + Title combination
                                        var exists = existingSchedules.Any(x => 
                                            x.RateId == rate.RateId && 
                                            x.StateId == state.StateId && 
                                            x.SerNo == sro.SerNo.Value && 
                                            x.Title == sro.Title);
                                        
                                        if (!exists)
                                        {
                                            var schedule = new ISroSchedule
                                            {
                                                RateId = rate.RateId, // Store RateId (PK from itaxrate table)
                                                StateId = state.StateId,
                                                SerNo = sro.SerNo.Value, // Can be 0, which is valid
                                                Title = sro.Title!,
                                                IsDeleted = 0
                                            };

                                            await _db.ISroSchedules.AddAsync(schedule);
                                            await _db.SaveChangesAsync();
                                            totalInserted++;

                                            existingSchedules.Add(new { schedule.RateId, schedule.StateId, schedule.SerNo, schedule.Title });
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Store exception with endpoint URL and break out of nested loops
                            // This will stop processing more combinations and move to next method
                            log.ISroScheduleCount = -1;
                            log.ISroScheduleError = ex.Message; // Already includes endpoint URL from MakeApiCall
                            return; // Exit method immediately, move to next method
                        }
                    }
                }

                log.ISroScheduleCount = totalInserted;
            }
            catch (Exception ex)
            {
                log.ISroScheduleCount = -1;
                log.ISroScheduleError = ex.Message; // Already includes endpoint URL from MakeApiCall
            }
        }

        private async Task FetchSroItems(string token, FBRDataFetchLog log)
        {
            try
            {
                // We need to get the FBR's sro_id, not our auto-generated one
                // So we'll query SroSchedule API again to get the mapping
                var taxRates = await _db.ITaxRates.Where(x => x.IsDeleted == 0).ToListAsync();
                var states = await _db.IStateTypes.Where(x => x.IsDeleted == 0).ToListAsync();
                var sroSchedules = await _db.ISroSchedules.Where(x => x.IsDeleted == 0).ToListAsync();
                
                // Format: "2025-03-25" -> "yyyy-MM-dd"
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
                var pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);
                var currentDate = pakistanTime.ToString("yyyy-MM-dd");
                var scheduleApiDate = pakistanTime.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                int totalInserted = 0;
                // Check uniqueness based on SROItemId (API's srO_ITEM_ID) + SroId combination
                var existingItems = await _db.ISroItemCodes
                    .Where(x => x.IsDeleted == 0)
                    .Select(x => new { x.SROItemId, x.SroId })
                    .ToListAsync();

                // Build mapping of (RateId PK, StateId, SerNo, Title) -> Our SroId
                // Handle duplicates by taking the first SroId when multiple schedules have the same key
                var scheduleMapping = sroSchedules
                    .GroupBy(s => $"{s.RateId}_{s.StateId}_{s.SerNo}_{s.Title}")
                    .ToDictionary(g => g.Key, g => g.First().SroId);

                foreach (var rate in taxRates)
                {
                    foreach (var state in states)
                    {
                        try
                        {
                            // Get SRO schedules for this rate/state to get FBR's sro_id
                            // Use TaxRateId (from API's ratE_ID) for the API call
                            // rate.RateId is the PK, rate.TaxRateId is the business key from API
                            var scheduleUrl = $"{BaseUrl}/SroSchedule?rate_id={rate.TaxRateId}&date={scheduleApiDate}&origination_supplier_csv={state.StateId}";
                            var scheduleResponse = await MakeApiCall<List<FBRSroScheduleResponse>>(scheduleUrl, token);

                            if (scheduleResponse != null && scheduleResponse.Any())
                            {
                                foreach (var apiSchedule in scheduleResponse)
                                {
                                    if (apiSchedule.SroId.HasValue && apiSchedule.SerNo.HasValue && !string.IsNullOrWhiteSpace(apiSchedule.Title))
                                    {
                                        // Build key to match our database schedule: RateId (PK)_StateId_SerNo_Title
                                        var key = $"{rate.RateId}_{state.StateId}_{apiSchedule.SerNo.Value}_{apiSchedule.Title}";
                                        
                                        // Find our database SroId by matching the schedule characteristics
                                        if (scheduleMapping.TryGetValue(key, out var ourSroId))
                                        {
                                            // Now fetch items using FBR's sro_id (from API response)
                                            var itemUrl = $"{BaseUrlV2}/SROItem?date={currentDate}&sro_id={apiSchedule.SroId.Value}";
                                            var itemResponse = await MakeApiCall<List<FBRSroItemResponse>>(itemUrl, token);

                                            if (itemResponse != null && itemResponse.Any())
                                            {
                                                // Filter items that don't already exist
                                                // Check uniqueness based on SROItemId (API's srO_ITEM_ID) + SroId combination
                                                // SroItemTitle can be duplicated for different SroId values
                                                var newItems = itemResponse
                                                    .Where(x => x.SroItemId.HasValue && !string.IsNullOrWhiteSpace(x.SroItemTitle))
                                                    .Where(x => !existingItems.Any(e => e.SROItemId == x.SroItemId.Value && e.SroId == ourSroId))
                                                    .Select(x => new ISroItemCode
                                                    {
                                                        // SchItemId is auto-increment primary key, don't set it
                                                        SROItemId = x.SroItemId!.Value, // Store API's srO_ITEM_ID in sroitemid column
                                                        SroItemTitle = x.SroItemTitle!, // Store API's srO_ITEM_DESC in sroitemtitle column
                                                        SroId = ourSroId, // Store parameter-based sroId in SroId column
                                                        IsDeleted = 0
                                                    })
                                                    .ToList();

                                                if (newItems.Any())
                                                {
                                                    await _db.ISroItemCodes.AddRangeAsync(newItems);
                                                    await _db.SaveChangesAsync();
                                                    totalInserted += newItems.Count;
                                                    // Update existing items list to avoid duplicates in this run
                                                    existingItems.AddRange(newItems.Select(x => new { x.SROItemId, x.SroId }));
                                                }
                                            }
                                        }
                                        // If schedule not found in mapping, it might not have been saved yet
                                        // This is expected if FetchSroItems runs before schedules are fully saved
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Store exception with endpoint URL and break out of nested loops
                            // This will stop processing more combinations and move to next method
                            log.ISroItemCodeCount = -1;
                            log.ISroItemCodeError = ex.Message; // Already includes endpoint URL from MakeApiCall
                            return; // Exit method immediately, move to next method
                        }
                    }
                }

                log.ISroItemCodeCount = totalInserted;
            }
            catch (Exception ex)
            {
                log.ISroItemCodeCount = -1;
                log.ISroItemCodeError = ex.Message; // Already includes endpoint URL from MakeApiCall
            }
        }

        private async Task<T?> MakeApiCall<T>(string url, string token)
        {
            const int timeoutMinutes = 5;
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(timeoutMinutes));
            
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request, cts.Token);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                    // Don't use PropertyNamingPolicy since we're using [JsonPropertyName] attributes
                };

                return JsonSerializer.Deserialize<T>(responseBody, options);
            }
            catch (TaskCanceledException ex) when (cts.Token.IsCancellationRequested)
            {
                throw new TimeoutException($"API call timed out after {timeoutMinutes} minutes. Endpoint: {url}", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"HTTP request failed. Endpoint: {url}. Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // If it's already a TimeoutException, preserve it with the URL
                if (ex is TimeoutException)
                {
                    throw;
                }
                throw new Exception($"API call failed. Endpoint: {url}. Error: {ex.Message}", ex);
            }
        }
    }
}

