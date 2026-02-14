using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("orgappsetting")]
    public class OrgAppSetting
    {
        [Key]
        [Column("settingid")]
        public int SettingId { get; set; }

        [Column("organizationname")]
        public string? OrganizationName { get; set; }

        [Column("website")]
        public string? Website { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("city")]
        public string? City { get; set; }

        [Column("stateid")]
        public int? StateId { get; set; }

        [Column("country")]
        public string? Country { get; set; }

        [Column("postalzipcode")]
        public string? PostalZipCode { get; set; }

        [Column("taxid")]
        public string? TaxId { get; set; }

        [Column("registrationnum")]
        public string? RegistrationNum { get; set; }

        [Column("currencyid")]
        public int? CurrencyId { get; set; }

        [Column("organizationlogo")]
        public string? OrganizationLogo { get; set; }

        [Column("fbrsandboxtoken")]
        public string? FbrSandboxToken { get; set; }

        [Column("fbrproductiontoken")]
        public string? FbrProductionToken { get; set; }

        [Column("isdeleted")]
        public int IsDeleted { get; set; }

        [Column("tenantid")]
        public int TenantId { get; set; }

        [Column("createdby")]
        public int CreatedBy { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; }

        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }

        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }
        [Column("invoicecolor")]
        public string? InvoiceColor { get; set; }

        [Column("isProduction")]
        public bool isProduction { get; set; }
    }
}
