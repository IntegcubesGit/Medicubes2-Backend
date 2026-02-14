namespace Application.Common.Interfaces
{
    public interface IDashboardService
    {
        Task<object> GetCustomerGrowth();
        Task<object> GetProductStatusDistribution();
        Task<object> GetTotalSaleAndUser();

    }
}
