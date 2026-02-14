using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface ICommonService
    {
        Task<object> GetConfigSettings();
        Task<object> GetSaleDeliveryList();
        Task<object> GetAllLocations();
        Task<object> GetSaleDeliveryInInvoiceList();
        Task<object> GetTaxRate(int stateId, int saleTypeId);
        Task<object> GetSROSchedule(int stateId, int rateId);
        Task<object> GetSROItemCodes(int sroId);
        Task<object> GetAllInvoiceTypes();
    }
}
