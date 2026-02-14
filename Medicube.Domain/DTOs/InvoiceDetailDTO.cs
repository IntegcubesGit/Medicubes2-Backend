namespace Domain.DTOs
{
    public class InvoiceDetailDTO
    {
        public int InvoiceDtlId { get; set; }
        public int InvoiceId { get; set; }
        public int ItemId { get; set; } // Product/Item ID
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; } = 0;
        public decimal StRate { get; set; } = 0;
        public int? RateId { get; set; }
        public int? SroId { get; set; }
        public int? SroItemId { get; set; } // ItemId from isroitemcode table (maps to ItemId in entity)
    }
}
