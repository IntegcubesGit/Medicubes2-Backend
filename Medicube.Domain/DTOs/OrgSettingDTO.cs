using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class OrgSettingDTO
    {
        public int SettingId { get; set; }
        public string? OrganizationName { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public int? StateId { get; set; }
        public string? Country { get; set; }
        public string? PostalZipCode { get; set; }
        public string? TaxId { get; set; }
        public string? RegistrationNum { get; set; }
        public int? CurrencyId { get; set; }
        public string? OrganizationLogo { get; set; }
        public string? FbrSandboxToken { get; set; }
        public string? FbrProductionToken { get; set; }
        public string? InvoiceColor { get;set; }

        public Boolean IsProduction { get; set; }
    }
}
