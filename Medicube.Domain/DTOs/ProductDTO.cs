namespace Domain.DTOs
{
    public class ProductDTO
    {
        public int itemid {  get; set; }
        public string? name { get; set; }
        public string? code { get; set; }
        public int? hscodeid { get; set; }
        public int? unitid { get; set; }
        public decimal? price { get; set; }
        public int? reorderlevel { get; set; }
        public int isactive { get; set; } = 0;
    }
}
