using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.CloudCode;

namespace Monopoly.Backend.Gateway.Services.Account
{
    public sealed class AccountServiceBinding
    {
        readonly ICloudCodeService k_Service;

        public AccountServiceBinding(ICloudCodeService service)
        {
            k_Service = service;
        }

        public async Task<string> PostAccount(string nickname, string trophies)
        {
            return await k_Service.CallModuleEndpointAsync<string>(
                "AccountService",
                "PostAccount",
                new Dictionary<string, object>()
                {
                    {"nickname", nickname},
                    {"trophies", trophies},
                });
        }

        public async Task<string> GetAccount()
        {
            return await k_Service.CallModuleEndpointAsync<string>(
                "AccountService",
                "GetAccount",
                new Dictionary<string, object>()
                {
                });
        }

        public async Task<string> GetAccountNickname()
        {
            return await k_Service.CallModuleEndpointAsync<string>(
                "AccountService",
                "GetAccountNickname",
                new Dictionary<string, object>()
                {
                });
        }

        public async Task<string> GetAccountTrophies()
        {
            return await k_Service.CallModuleEndpointAsync<string>(
                "AccountService",
                "GetAccountTrophies",
                new Dictionary<string, object>()
                {
                });
        }

        public async Task<string> PutAccountNickname(string nickname)
        {
            return await k_Service.CallModuleEndpointAsync<string>(
                "AccountService",
                "PutAccountNickname",
                new Dictionary<string, object>()
                {
                    {"nickname", nickname},
                });
        }

        public async Task<string> PutAccountTrophies(string trophies)
        {
            return await k_Service.CallModuleEndpointAsync<string>(
                "AccountService",
                "PutAccountTrophies",
                new Dictionary<string, object>()
                {
                    {"trophies", trophies},
                });
        }
    }
}
