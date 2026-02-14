namespace Domain.DTOs
{
    public class GetInvoiceDetailRequestDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int[] CustomerIds { get; set; } = Array.Empty<int>();
    }
}
