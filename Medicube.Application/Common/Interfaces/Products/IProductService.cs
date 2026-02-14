using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;
using Domain.Entities;

namespace Application.Common.Interfaces.Products
{
    public interface IProductService
    {
        Task<object> GetAllProduct(int StatusId);
        Task<object> GetProductById(int ItemId);
        Task<object> AddProduct(ProductDTO dto);
        Task<object> SaveAllProducts(List<ProductDTO> list);
        Task<object> AddProductStockIn(ProductStockDTO dto);
        Task<object> ProductStockOut(ProductStockDTO dto);
        Task<object> UpdateProduct(ProductDTO dto);
        Task<object> DeleteProduct(int ItemId);
        Task<object> GetHSCode();
        Task<object> GetSaleType();
        Task<object> GetUnitType();
        Task<object> GetProductCount();
        Task<object> GetArchivedList();
        Task<object> UpdateArchivedById(int ItemId);

    }
}
