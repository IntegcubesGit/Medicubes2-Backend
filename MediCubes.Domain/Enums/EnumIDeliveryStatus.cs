using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public class EnumIDeliveryStatus
    {
        public enum IDeliveryStatus
        {
            Draft = 1,
            Packed = 2,
            Shipped = 3,
            Delivered = 4,
            Cancelled = 5
        }
    }

}
