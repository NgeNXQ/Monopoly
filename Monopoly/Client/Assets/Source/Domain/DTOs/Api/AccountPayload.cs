using Newtonsoft.Json;

namespace Monopoly.Domain.DTOs.Api
{
    public struct AccountPayload
    {
        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("trophies")]
        public string Trophies { get; set; }
    }
}
