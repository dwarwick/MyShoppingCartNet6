using Newtonsoft.Json;
using System.Xml.Serialization;

namespace MyShoppingCart.Models.USPS
{
    public class SpecialService
    {
        [XmlElement("ServiceID")]
        [JsonProperty("ServiceID")]
        public int ServiceID { get; set; }

        [XmlElement("ServiceName")]
        [JsonProperty("ServiceName")]
        public string ServiceName { get; set; }

        [XmlElement("Available", DataType = "boolean")]
        [JsonProperty("Available")]
        public bool Available { get; set; }

        [XmlElement("Price", DataType = "decimal")]
        [JsonProperty("Price")]
        public decimal Price { get; set; }
    }
}
