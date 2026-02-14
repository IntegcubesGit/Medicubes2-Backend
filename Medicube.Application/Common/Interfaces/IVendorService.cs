using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Application.Common.Interfaces
{
    public interface IVendorService
    {
        Task<object> GetAllVendor(int vendorTypeId,int StateTypeId);
        Task<object> GetVendorById(int VendorId);
        Task<object> AddVendor(VendorDTO dto);
        Task<object> UpdateVendor(VendorDTO dto);
        Task<object> DeleteVendor(int VendorId);
        Task<object> GetVendorType();
        Task<object> GetStateType();
        Task<object> GetArchivedList();
        Task<object> UpdateArchivedById(int VendorId);

    }
}
