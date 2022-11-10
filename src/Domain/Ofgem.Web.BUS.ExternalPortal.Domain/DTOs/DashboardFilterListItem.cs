using Ofgem.API.BUS.Applications.Domain.Entities.Enums;

namespace Ofgem.Web.BUS.ExternalPortal.Domain.DTOs;

public record DashboardFilterListItem
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public IEnumerable<Guid>? StatusIds { get; set; }
    public ConsentState ConsentState { get; set; }
}
