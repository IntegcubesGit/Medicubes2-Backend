using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public class EnumInvoiceStatus
    {
        public enum InvoiceStatus
        {
            Draft = 1,
            Pending = 2,
            SubmittedToFBR = 3,
            Approved = 4,
            Rejected = 5,
            Cancelled = 6
        };
    }
}
