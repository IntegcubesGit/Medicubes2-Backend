using System.Data;
using Application.Common.Interfaces.JWT;
using Application.Common.Interfaces.Products;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Index.HPRtree;

namespace Infrastructure.Repositories.Products
{
    public class ProductRepository : IProductService
    {
        private readonly AppDbContext _db;
        private readonly IJWTService _jwtService;

        public ProductRepository(AppDbContext context, IJWTService jWTService)
        {
            _db = context;
            _jwtService = jWTService;
        }
        public async Task<object> AddProduct(ProductDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();

            var IsProductExist = await _db.Products.Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId && a.code == dto.code).FirstOrDefaultAsync();
            if (IsProductExist != null)
            {
                return new
                {
                    success = false,
                    message = "Product already exist!"
                };
            }
            var entity = new Product
            {
                name = dto.name,
                code = dto.code,
                hscodeid = dto.hscodeid,
                unitid = dto.unitid,
                price = dto.price,
                reorderlevel = dto.reorderlevel,
                createdon = DateTime.UtcNow,
                createdby = tempUser.UserId,
                tenantid = tempUser.TenantId,
                isactive = dto.isactive,
                isdeleted = 0
            };

            await _db.Products.AddAsync(entity);
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Product added successfully",
                data = entity
            };
        }

        public async Task<object> GetAllProduct(int StatusId)
        {
            var tempUser = _jwtService.DecodeToken();

            var query = from p in _db.Products where p.isdeleted == 0 && p.tenantid == tempUser.TenantId
                join inv in _db.InvInventories on p.itemid equals inv.ItemId into invGroup
                from inv in invGroup.DefaultIfEmpty()
                group inv by new
                {
                    p.itemid,
                    p.name,
                    p.code,
                    p.hscodeid,
                    p.unitid,
                    p.price,
                    p.isactive,
                    p.reorderlevel
                } into g
                select new
                {
                    g.Key.itemid,
                    g.Key.name,
                    g.Key.code,
                    g.Key.hscodeid,
                    g.Key.unitid,
                    g.Key.price,
                    g.Key.isactive,
                    g.Key.reorderlevel,
                    currentstock = g.Sum(x => (decimal?)(x.Quantity - x.ConsumedQuantity) ?? 0)
                };

            if (StatusId == 0)
                query = query.Where(x => x.isactive == 0);

            else if (StatusId == 1)
                query = query.Where(x => x.isactive == 1);

            else if (StatusId == 2)
                query = query.Where(x => x.currentstock < x.reorderlevel);

            var res = await query.OrderByDescending(s => s.itemid).ToListAsync();

            return res;
        }



        public async Task<object> GetProductById(int ItemId)
        {
            var tempUser = _jwtService.DecodeToken();
            var res = await (from p in _db.Products.Where(a => a.itemid == ItemId && a.isdeleted == 0 && a.tenantid == tempUser.TenantId)
                             select new
                             {
                                 p.itemid,
                                 p.name,
                                 p.code,
                                 p.hscodeid,
                                 p.unitid,
                                 p.price,
                                 p.reorderlevel,
                                 p.isactive,
                                 p.createdon,
                                 p.createdby,
                                 p.tenantid
                             }).FirstOrDefaultAsync();
            if (res == null)
            {
                return new
                {
                    statusCode = 200,
                    message = "Data not Found"
                };
            }
            return res;
        }

        public async Task<object> UpdateProduct(ProductDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();

            var existingItem = await _db.Products.FirstOrDefaultAsync(a => a.itemid == dto.itemid && a.isdeleted == 0 && a.tenantid == tempUser.TenantId);

            if (existingItem == null)
            {
                return new
                {
                    message = "Record not found!",
                    statusCode = 200,
                    success = false
                };
            }

            existingItem.name = dto.name;
            existingItem.code = dto.code;
            existingItem.hscodeid = dto.hscodeid;
            existingItem.unitid = dto.unitid;
            existingItem.price = dto.price;
            existingItem.reorderlevel = dto.reorderlevel;
            existingItem.modifiedon = DateTime.UtcNow;
            existingItem.modifiedby = tempUser.UserId;
            existingItem.tenantid = tempUser.TenantId;
            existingItem.isactive = dto.isactive;
            existingItem.isdeleted = 0;

            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Product updated successfully",
                data = existingItem
            };
        }

        public async Task<object> DeleteProduct(int ItemId)
        {
            var tempUser = _jwtService.DecodeToken();

            var existingItem = await _db.Products.FirstOrDefaultAsync(a => a.itemid == ItemId && a.isdeleted == 0 && a.tenantid == tempUser.TenantId);

            if (existingItem == null)
            {
                return new
                {
                    message = "Record not found!",
                    statusCode = 200,
                    success = false
                };
            }

            existingItem.isdeleted = 1;
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Product delete successfully",
                data = existingItem
            };
        }

        public async Task<object> GetHSCode()
        {
            var tempUser = _jwtService.DecodeToken();
            var list = await _db.IHSCodes.Where(a => a.IsDeleted == 0).Select(a => new { a.HSCodeId, a.Code, a.description }).ToListAsync();
            return list;
        }

        public async Task<object> GetSaleType()
        {
            var tempUser = _jwtService.DecodeToken();
            var list = await _db.ISaleTypes.Where(a => a.IsDeleted == 0).Select(a => new { a.SaleTypeId, a.SaleName }).ToListAsync();
            return list;
        }

        public async Task<object> GetUnitType()
        {
            var tempUser = _jwtService.DecodeToken();
            var list = await _db.IUnitTypes.Where(a => a.IsDeleted == 0).Select(a => new { a.UnitId, a.UnitName }).ToListAsync();
            return list;
        }

        public async Task<object> AddProductStockIn(ProductStockDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();
            var checkItemID = _db.Products.Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId && a.itemid == dto.ItemId).FirstOrDefault();
            if (checkItemID == null)
            {
                return new
                {
                    success = false,
                    message = "Product not found!",
                    statusCode = 200
                };
            }
            InvInventory inv = new InvInventory();
            inv.StoreId = 1;
            inv.ItemId = dto.ItemId;
            inv.Quantity = dto.Qty;
            inv.Price = dto.UnitPrice;
            inv.ConsumedQuantity = 0;
            inv.OrgId = 1;
            inv.ItemTransId = 1;
            inv.ExtId = 0;
            inv.TransDate = DateTime.UtcNow;
            inv.TenantId = tempUser.TenantId;
            await _db.InvInventories.AddAsync(inv);
            await _db.SaveChangesAsync();

            InvStoreStock stock = new InvStoreStock();
            stock.TransDate = DateTime.UtcNow;
            stock.StoreId = 1;
            stock.ItemId = dto.ItemId;
            stock.Quantity = dto.Qty;
            stock.ItemTransId = 1;
            stock.ExtId = 0;
            stock.CreatedBy = tempUser.UserId;
            stock.CreatedOn = DateTime.UtcNow;
            stock.OrgId = 1;
            stock.Price = dto.UnitPrice;
            stock.InvId = inv.InvId;
            stock.PurchsePrice = dto.UnitPrice;
            stock.LocId = 1;
            stock.Reference = dto.References;
            stock.Notes = dto.Notes;
            await _db.InvStoreStocks.AddAsync(stock);

            StockHistoryLogs logs = new StockHistoryLogs();
            logs.ProdId = dto.ItemId;
            logs.StockQty = dto.Qty;
            logs.UnitPrice = dto.UnitPrice;
            logs.Reference = dto.References  == null ? "N/A" : dto.References;
            logs.Notes = dto.Notes == null ? "N/A" : dto.Notes;
            logs.CreatedOn = DateTime.UtcNow;
            logs.CreatedBy = tempUser.UserId;
            await _db.StockHistoryLogs.AddAsync(logs);

            await _db.SaveChangesAsync();
            return new
            {
                success = true,
                message = "Stock added successfully",
                statusCode = 200
            };
        }
        public async Task<object> ProductStockOut(ProductStockDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();
            var data = (from inv in _db.InvInventories.Where(a => a.StoreId == 1)
                        where inv.ItemId == dto.ItemId && inv.Quantity != inv.ConsumedQuantity
                        select new
                        {
                            inv.InvId,
                            inv.ItemId,
                            inv.Quantity,
                            inv.Price,
                            inv.ConsumedQuantity,
                            inv.TransDate
                        }).OrderBy(a => a.TransDate).ToList();

            decimal newIssuedQty = dto.Qty;
            foreach (var inv in data)
            {
                decimal DifferQuantity = 0;

                DifferQuantity = inv.Quantity - inv.ConsumedQuantity;

                if (newIssuedQty > DifferQuantity)
                {

                    InvStoreStock stock = new InvStoreStock();
                    stock.TransDate = DateTime.UtcNow;
                    stock.StoreId = 1;
                    stock.ItemId = dto.ItemId;
                    stock.Quantity = (-1) * (decimal)DifferQuantity;
                    stock.Price = inv.Price;

                    stock.PurchsePrice = stock.Price;

                    stock.ItemTransId = 1;
                    stock.ExtId = 0;
                    stock.CreatedOn = DateTime.UtcNow;
                    stock.CreatedBy = tempUser.UserId;
                    stock.OrgId = 1;
                    stock.InvId = inv.InvId;
                    stock.Reference = dto.References;
                    stock.Notes = dto.Notes;
                    await _db.InvStoreStocks.AddAsync(stock);
                    await _db.SaveChangesAsync();

                    InvInventory update = await (from udpInv in _db.InvInventories where udpInv.InvId == inv.InvId select udpInv).SingleOrDefaultAsync();
                    if (update != null)
                    {
                        update.ConsumedQuantity = update.ConsumedQuantity + DifferQuantity;
                        await _db.SaveChangesAsync();
                    }

                    StockHistoryLogs logs = new StockHistoryLogs();
                    logs.ProdId = inv.ItemId;
                    logs.StockQty = (-1) * (decimal)DifferQuantity;
                    logs.UnitPrice = inv.Price.HasValue ? 
                                        Math.Round(inv.Price.Value, 2, MidpointRounding.AwayFromZero) : 
                                        null;
                    logs.Reference = dto.References == null ? "N/A" : dto.References;
                    logs.Notes = dto.Notes == null ? "N/A" : dto.Notes;
                    logs.CreatedOn = DateTime.UtcNow;
                    logs.CreatedBy = tempUser.UserId;
                    await _db.StockHistoryLogs.AddAsync(logs);
                    await _db.SaveChangesAsync();

                    newIssuedQty = newIssuedQty - DifferQuantity;

                }
                else
                {


                    InvStoreStock stock = new InvStoreStock();
                    stock.TransDate = DateTime.UtcNow;
                    stock.StoreId = 1;
                    stock.ItemId = dto.ItemId;
                    stock.Quantity = (-1) * (decimal)newIssuedQty;
                    stock.Price = inv.Price;
                    stock.PurchsePrice = stock.Price;
                    stock.ItemTransId = 1;
                    stock.ExtId = 0;
                    stock.CreatedOn = DateTime.UtcNow;
                    stock.CreatedBy = tempUser.UserId;
                    stock.OrgId = 1;
                    stock.InvId = inv.InvId;
                    stock.Reference = dto.References;
                    stock.Notes = dto.Notes;
                    await _db.InvStoreStocks.AddAsync(stock);
                    await _db.SaveChangesAsync();

                    InvInventory update = await (from udpInv in _db.InvInventories where udpInv.InvId == inv.InvId select udpInv).SingleOrDefaultAsync();


                    update.ConsumedQuantity = update.ConsumedQuantity + newIssuedQty;
                    await _db.SaveChangesAsync();
                    //  inv.ConsumedQuantity = inv.ConsumedQuantity + DifferQuantity;

                    StockHistoryLogs logs = new StockHistoryLogs();
                    logs.ProdId = dto.ItemId;
                    logs.StockQty = (-1) * (decimal)newIssuedQty;
                    logs.UnitPrice = inv.Price.HasValue ?
                                     Math.Round(inv.Price.Value, 2, MidpointRounding.AwayFromZero) :
                                     null;
                    logs.Reference = dto.References == null ? "N/A" : dto.References;
                    logs.Notes = dto.Notes == null ? "N/A" : dto.Notes;
                    logs.CreatedOn = DateTime.UtcNow;
                    logs.CreatedBy = tempUser.UserId;
                    await _db.StockHistoryLogs.AddAsync(logs);
                    await _db.SaveChangesAsync();


                    newIssuedQty = newIssuedQty - newIssuedQty;

                }
            }
            return new
            {
                success = true,
                message = "Stock Out Successfully",
                statusCode = 200
            };
        }

        public async Task<object> GetProductCount()
        {
            var tempUser = _jwtService.DecodeToken();

            var productCount = await _db.Products.Where(p => p.isdeleted == 0 && p.tenantid == tempUser.TenantId).CountAsync();

            var stockData = await (from p in _db.Products
                                   where p.isdeleted == 0 && p.tenantid == tempUser.TenantId
                                   join inv in _db.InvInventories on p.itemid equals inv.ItemId into invGroup
                                   from ig in invGroup.DefaultIfEmpty()
                                   group ig by new { p.itemid, p.reorderlevel } into g
                                   select new
                                   {
                                       ItemId = g.Key.itemid,
                                       ReorderLevel = g.Key.reorderlevel,
                                       AvailableQty = g.Sum(x => (decimal?)(x.Quantity - x.ConsumedQuantity) ?? 0)
                                   }).ToListAsync();

            var outOfStockCount = stockData.Count(x => x.AvailableQty == 0);
            var lowStockCount = stockData.Count(x => x.AvailableQty > 0 && x.AvailableQty < x.ReorderLevel);
            var inStockCount = stockData.Count(x => x.AvailableQty >= x.ReorderLevel);

            return new
            {
                ProductCount = productCount,
                LowStockCount = lowStockCount,
                OutOfStockCount = outOfStockCount,
                InStockCount = inStockCount
            };
        }


        public async Task<object> GetArchivedList()
        {
            var tempUser = _jwtService.DecodeToken();
            var res = await _db.Products.Where(a => a.isdeleted == 1 && a.tenantid == tempUser.TenantId).Select(a => new
            {
                a.itemid,
                a.name,
                a.code,
                a.hscodeid,
                a.unitid,
                a.price,
                a.isactive,
                a.reorderlevel
            }).OrderByDescending(s => s.itemid).ToListAsync();
            return res;
        }

        public async Task<object> UpdateArchivedById(int ItemId)
        {
            var tempUser = _jwtService.DecodeToken();

            var existingItem = await _db.Products.FirstOrDefaultAsync(a => a.itemid == ItemId && a.isdeleted == 1 && a.tenantid == tempUser.TenantId);

            if (existingItem == null)
            {
                return new
                {
                    message = "Record not found!",
                    statusCode = 200,
                    success = false
                };
            }

            existingItem.isdeleted = 0;
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Product restore successfully",
                data = existingItem
            };
        }

        public async Task<object> SaveAllProducts(List<ProductDTO> list)
        {
            var tempUser = _jwtService.DecodeToken();
            var tenantId = tempUser.TenantId;

            var incomingCodes = list.Select(x => x.code).ToList();

            var existingProducts = await _db.Products.Where(p => p.isdeleted == 0 && p.tenantid == tenantId && incomingCodes.Contains(p.code))
                .Select(p => p.code)
                .ToListAsync();

            var newProducts = list.Where(x => !existingProducts.Contains(x.code)).ToList();

            if (!newProducts.Any())
            {
                return new
                {
                    success = false,
                    message = "All products already exist!"
                };
            }

            var entities = newProducts.Select(dto => new Product
            {
                name = dto.name,
                code = dto.code,
                hscodeid = dto.hscodeid,
                unitid = dto.unitid,
                price = dto.price,
                reorderlevel = dto.reorderlevel,
                createdon = DateTime.UtcNow,
                createdby = tempUser.UserId,
                tenantid = tenantId,
                isactive = dto.isactive,
                isdeleted = 0
            }).ToList();

            // Bulk Insert
            await _db.Products.AddRangeAsync(entities);
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = $"{entities.Count} products added successfully",
                data = entities
            };
        }

    }
}
