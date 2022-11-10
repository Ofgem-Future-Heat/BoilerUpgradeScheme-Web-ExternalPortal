namespace Ofgem.Web.BUS.ExternalPortal.Domain.Concrete
{
    public class PageHistoryModel
    {
        public string? CurrentPagePath { get; set; }

        public List<string> PageHistory { get; set; } = new();
    }
}
