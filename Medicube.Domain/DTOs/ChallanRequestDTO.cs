namespace Domain.DTOs
{
    public class ChallanRequestDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? RefNumber { get; set; }
        public string? PSID { get; set; }
        public int InvoiceId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public int StatusId { get; set; }
        public int CustomerId { get; set; }
        public int TaxChallanId { get; set; }
    }
}
