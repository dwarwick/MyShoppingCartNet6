using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class CertificateExistResponse
    {
        [JsonProperty("value")]
        public List<CertificateClassMetadata> value { get; set; }

        
    }

    public class CertificateClassMetadata
    {
        [JsonProperty("id")]
        public string  id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("location")]
        public string location { get; set; }
        [JsonProperty("properties")]
        public CertificateProperties properties { get; set; }

    }

    public class CertificateProperties
    {
        [JsonProperty("friendlyName")]
        public string friendlyName { get; set; }
        [JsonProperty("subjectName")]
        public string subjectName { get; set; }
        [JsonProperty("hostNames")]
        public List<string> hostNames { get; set; }
        [JsonProperty("issuer")]
        public string  issuer { get; set; }
        [JsonProperty("expirationDate")]
        public string  expirationDate { get; set; }
        [JsonProperty("thumbprint")]
        public string thumbprint { get; set; }

    }
}
