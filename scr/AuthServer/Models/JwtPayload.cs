using Newtonsoft.Json;

namespace AuthServer.Models
{
    public class JwtPayload
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
