using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class RootObject
    {
        [JsonProperty("Records")]
        public List<AddressBody> Data { get; set; }
        [JsonProperty("TotalRecords")]
        public string TotalRecords { get; set; }
        [JsonProperty("TransmissionReference")]
        public string TransmissionReference { get; set; }
        [JsonProperty("TransmissionResults")]
        public string TransmissionResults { get; set; }
        [JsonProperty("Version")]
        public string Version { get; set; }
    }


    

    public class AddressBody
    {
        [JsonProperty("AddressExtras")]
        public string AddressExtras { get; set; }
        [JsonProperty("AddressKey")]
        public string AddressKey { get; set; }        
        [JsonProperty("AddressLine1")]
        public string AddressLine1 { get; set; }
        [JsonProperty("AddressLine2")]
        public string AddressLine2 { get; set; }
        [JsonProperty("City")]
        public string City { get; set; }
        [JsonProperty("State")]
        public string State { get; set; }
        [JsonProperty("PostalCode")]
        public string PostalCode { get; set; }
        [JsonProperty("CompanyName")]
        public string CompanyName { get; set; }
        [JsonProperty("EmailAddress")]
        public string EmailAddress { get; set; }
        [JsonProperty("MelissaAddressKey")]
        public string MelissaAddressKey { get; set; }
        [JsonProperty("MelissaAddressKeyBase")]
        public string MelissaAddressKeyBase { get; set; }
        [JsonProperty("NameFull")]
        public string NameFull { get; set; }
        [JsonProperty("PhoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonProperty("RecordExtras")]
        public string RecordExtras { get; set; }
        [JsonProperty("RecordID")]
        public string RecordID { get; set; }
        [JsonProperty("Reserved")]
        public string Reserved { get; set; }
        [JsonProperty("Results")]
        public string Results { get; set; }
    }

    
}
