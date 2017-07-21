using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace adssys.Models
{
    public class Provider
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } 

        [JsonProperty("secret-key")]
        public string SecretKey { get; set; }

        [JsonProperty("ads-id")]
        public string AdsId { get; set; }
    }
}
