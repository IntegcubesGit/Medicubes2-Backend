using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Application.Common.Interfaces.Customers
{
    public interface ICustomerService
    {
        Task<object> GetAllCustomer(int custRegId, int custTypeId);
        Task<object> GetCustomerById(int CustId);
        Task<object> AddCustomer(CustomerDTO dto);
        Task<object> UpdateCustomer(CustomerDTO dto);
        Task<object> DeleteCustomer(int CustId);
        Task<object> GetCustRegType();
        Task<object> GetCustomerType();
        Task<object> GetArchivedList();
        Task<object> UpdateArchivedById(int CustId);
    }
}
