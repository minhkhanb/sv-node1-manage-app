using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace adssys.Models
{
    public class Delivery
    {
        [JsonProperty("provider-id")]
        public string providerId { get; set; }

        [JsonProperty("device-id")]
        public string deviceId { get; set; }

        [JsonProperty("salt")]
        public string salt { get; set; }

        [JsonProperty("sign")]
        public string sign { get; set; }
    }
}
