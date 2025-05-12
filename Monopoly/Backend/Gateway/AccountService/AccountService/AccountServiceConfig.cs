using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;
using Microsoft.Extensions.DependencyInjection;

namespace Monopoly.Backend.Gateway.Services.Account;

public sealed class AccountServiceConfig : ICloudCodeSetup
{
    public void Setup(ICloudCodeConfig config)
    {
        config.Dependencies.AddSingleton(GameApiClient.Create());
    }
}
