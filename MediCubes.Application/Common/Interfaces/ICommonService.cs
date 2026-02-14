namespace Application.Common.Interfaces
{
    public interface ICommonService
    {
        Task<object> GetConfigSettings();
        Task<object> GetAllLocations();
    }
}
