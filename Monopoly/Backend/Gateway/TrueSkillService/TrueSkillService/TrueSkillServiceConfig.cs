using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;

namespace Monopoly.Gateway;

public sealed class ModuleConfig : ICloudCodeSetup
{
    public void Setup(ICloudCodeConfig config)
    {
        config.Dependencies.AddSingleton(GameApiClient.Create());
    }
}
