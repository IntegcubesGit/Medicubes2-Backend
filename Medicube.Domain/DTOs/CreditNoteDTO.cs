namespace Domain.DTOs
{
    public class CreditNoteDTO
    {
        public int InvoiceId { get; set; }
        public int DeliveryId { get; set; }
        public int CustId { get; set; }
        public string? CreditNoteNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public string? Notes { get; set; }
        public int reasonTypeId { get; set; }
        public int returnItem { get; set; }
        public int statusId { get; set; }
        public int OriginationId { get; set; }
        public int DestinationId { get; set; }
        public string? InvoiceNumber { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int FBRTestScenarioId { get; set; }
        public string? PaymentTerm { get; set; }
        public List<InvoiceDetailDTO>? Details { get; set; }
    }
    public class CreditNoteListDTO
    {
        public int reasonTypeId { get; set; }
        public int customerId { get; set; }
        public int statusId { get; set; }
        public string? startDate {  get; set; }
        public string? endDate { get; set; }
    }

}
