namespace Ofgem.Web.BUS.ExternalPortal.Core.Interfaces
{
    public interface IGraphApiService
    {
        Task<bool> HasUserRegistered(string emailAddress);
    }
}
