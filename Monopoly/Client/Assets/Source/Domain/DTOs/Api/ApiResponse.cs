using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Monopoly.Domain.DTOs.Api
{
    public struct ApiResponse<TPayload>
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public TPayload Payload { get; set; }

        [JsonProperty("code")]
        public HttpStatusCode Code { get; set; }

        [JsonProperty("errors")]
        public List<string> Errors { get; set; }
    }
}
