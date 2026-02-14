using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs
{
    public class FBRInvoiceSubmissionDTO
    {
        [Required]
        public int InvoiceId { get; set; }
        public string InvoiceType { get; set; }

        [Required]
        public string InvoiceDate { get; set; }

        [Required]
        public string SellerNTNCNIC { get; set; }

        [Required]
        public string SellerBusinessName { get; set; }

        [Required]
        public string SellerProvince { get; set; }

        [Required]
        public string SellerAddress { get; set; }

        public string BuyerNTNCNIC { get; set; }
        public string BuyerBusinessName { get; set; }
        public string BuyerProvince { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerRegistrationType { get; set; }
        public string InvoiceRefNo { get; set; }
        public string ScenarioId { get; set; }

        [Required]
        public List<FBRInvoiceItemDTO> Items { get; set; }
    }
}
