using System.Text.Json.Serialization;

namespace Domain.DTOs
{
    // DTOs for FBR API Responses
    public class FBRHSCodeResponse
    {
        [JsonPropertyName("hS_CODE")]
        public string? HS_CODE { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class FBRStateResponse
    {
        [JsonPropertyName("stateProvinceCode")]
        public int StateProvinceCode { get; set; }

        [JsonPropertyName("stateProvinceDesc")]
        public string? StateProvinceDesc { get; set; }
    }

    public class FBRInvoiceTypeResponse
    {
        [JsonPropertyName("docTypeId")]
        public int DocTypeId { get; set; }

        [JsonPropertyName("docDescription")]
        public string? DocDescription { get; set; }
    }

    public class FBRSaleTypeResponse
    {
        [JsonPropertyName("transactioN_TYPE_ID")]
        public int TRANSACTION_TYPE_ID { get; set; }

        [JsonPropertyName("transactioN_DESC")]
        public string? TRANSACTION_DESC { get; set; }
    }

    public class FBRUoMResponse
    {
        [JsonPropertyName("uoM_ID")]
        public int UOM_ID { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class FBRTaxRateResponse
    {
        [JsonPropertyName("ratE_ID")]
        public int? RateId { get; set; }

        [JsonPropertyName("ratE_DESC")]
        public string? RateTitle { get; set; }

        [JsonPropertyName("ratE_VALUE")]
        public decimal? RateValue { get; set; }
    }

    public class FBRSroScheduleResponse
    {
        [JsonPropertyName("srO_ID")]
        public int? SroId { get; set; }

        [JsonPropertyName("serNo")]
        public int? SerNo { get; set; }

        [JsonPropertyName("srO_DESC")]
        public string? Title { get; set; }
    }

    public class FBRSroItemResponse
    {
        [JsonPropertyName("srO_ITEM_ID")]
        public int? SroItemId { get; set; }

        [JsonPropertyName("srO_ITEM_DESC")]
        public string? SroItemTitle { get; set; }
    }
}

