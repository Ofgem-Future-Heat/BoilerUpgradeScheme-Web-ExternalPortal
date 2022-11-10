using Ofgem.Web.BUS.ExternalPortal.Core.Interfaces;
using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Services
{
    public class GraphApiService : IGraphApiService
    {
        private readonly string _tenant;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public GraphApiService(IConfiguration config)
        {
            _tenant = config["AzureAdB2C:Domain"];
            _clientId = config["AzureAdB2C:ClientId"];
            _clientSecret = config["GraphApi:ClientSecret"];
        }

        public async Task<bool> HasUserRegistered(string emailAddress)
        {
            string tenant = _tenant;
            string clientId = _clientId;
            string clientSecret = _clientSecret;

            ClientSecretCredential clientSecretCredential = new(tenant, clientId, clientSecret);

            GraphServiceClient graphClient = new(clientSecretCredential);

            var results = await graphClient.Users
                                           .Request()
                                           .Filter($"identities/any(c:c/issuerAssignedId eq '{emailAddress}' and c/issuer eq '{tenant}')")
                                           .Select(e => e.Identities)
                                           .GetAsync();
            return results.Any();
        }
    }
}
