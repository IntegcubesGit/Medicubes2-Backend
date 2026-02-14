namespace Application.Common.Interfaces
{
    public interface IFBRDataFetchService
    {
        Task<object> FetchAllFBRData();
    }
}

