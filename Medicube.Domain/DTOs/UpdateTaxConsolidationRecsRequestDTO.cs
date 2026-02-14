namespace Domain.DTOs
{
    public class UpdateTaxConsolidationRecsRequestDTO
    {
        public required int TaxChallanId { get; set; }
        public required string CPRNNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public required string ChequeNo { get; set; }
        public string PaidRemarks { get; set; }
    }
}
