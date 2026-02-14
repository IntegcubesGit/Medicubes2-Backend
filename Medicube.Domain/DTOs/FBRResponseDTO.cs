using System.Text.Json.Nodes;

namespace Domain.DTOs
{
    public class FBRResponseDTO
    {
        public int InvoiceId { get; set; }
        public int StatusCode { get; set; }
        public string? FBRInvoiceNumber { get; set; }
        public JsonObject? response { get; set; }
    }

}
