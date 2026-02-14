using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class CustomerDTO
    {
        public int customerid { get; set; }
        public string? name { get; set; }
        public int customertypeid { get; set; }
        public int custregtypeid { get; set; }
        public string? regno { get; set; }
        public string? address { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public int stateprovince { get; set; }
        public string? cnic { get; set; }
    }
}
