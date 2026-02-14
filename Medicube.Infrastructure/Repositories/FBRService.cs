using Application.Common.Interfaces;
using Application.Common.Interfaces.JWT;
using Domain.DTOs;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class FBRService : IFBRService
    {
        private readonly AppDbContext _db;
        private readonly HttpClient _httpClient;
        private readonly IJWTService _jWTService;
        private readonly IConfiguration configuration;

        public FBRService(HttpClient httpClient, AppDbContext db, IJWTService jWTService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _db = db;
            _jWTService = jWTService;
            this.configuration = configuration;
        }

        public async Task<object> SubmitInvoiceToFBR(FBRInvoiceSubmissionDTO dto)
        {
            var tempUser = _jWTService.DecodeToken();
            var orgSettings = await _db.OrgAppSettings
                .Where(a => a.IsDeleted == 0 && a.TenantId == tempUser.TenantId)
                .FirstOrDefaultAsync();

          

            string url_sb = "https://gw.fbr.gov.pk/di_data/v1/di/postinvoicedata_sb";
            string url_prod = "https://gw.fbr.gov.pk/di_data/v1/di/postinvoicedata";
            string url = "";
            string token = "";
            bool IsProd = false;

            if (orgSettings != null)
            {
                IsProd = orgSettings.isProduction;

                if (IsProd)
                {
                    url = url_prod;
                    token = orgSettings.FbrProductionToken ?? "";

                }
                else
                {
                    url = url_sb;
                    token = orgSettings.FbrSandboxToken ?? "";
                }
            }
          
                // Get token from database
              
            if (orgSettings == null || string.IsNullOrWhiteSpace(token))
            {
                throw new Exception("FBR Sandbox Token not found in organization settings");
            }

            

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                // Set headers
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("User-Agent", "PostmanRuntime/7.36.0");
                request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

                // Serialize payload
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                string jsonPayload = JsonSerializer.Serialize(dto, options);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send request
                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                // Ensure success
                response.EnsureSuccessStatusCode();

                // Deserialize into a JsonDocument to check status
                using var jsonDoc = JsonDocument.Parse(responseBody);
                var root = jsonDoc.RootElement;
                bool isValid = false;

                if (root.TryGetProperty("validationResponse", out var validationResponse) &&
                    validationResponse.TryGetProperty("status", out var statusProp))
                {
                    isValid = statusProp.GetString() == "Valid";
                }

                string invoiceNumber = null;

                if (root.TryGetProperty("invoiceNumber", out var invoiceNumberProp))
                {
                    invoiceNumber = invoiceNumberProp.GetString();
                }

                // Save to DB 

                var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == dto.InvoiceId);
                 if (invoice != null)
                 {
                     invoice.FBRResponse = responseBody;
                     invoice.FBRInvoiceNumber = invoiceNumber;
                     invoice.StatusId = 3;
                     await _db.SaveChangesAsync();
                 }


                // Return the full response regardless of saving
                return !string.IsNullOrWhiteSpace(responseBody)
    ? new
    {
        isProd = IsProd,
        fbrResponse = JsonSerializer.Deserialize<object>(responseBody)
    }
    : new
    {
        isProduction = IsProd,
        success = true,
        message = "Request accepted but empty response",
        statusCode = response.StatusCode
    };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }
        }
    }
}