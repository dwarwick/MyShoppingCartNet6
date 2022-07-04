using System;
using System.Xml.Serialization;

namespace MyShoppingCart.Models.USPS
{
    [Serializable()]
    public class Postage
    {
        [XmlElement("MailService")]
        public string MailService { get; set; }

        [XmlElement("Rate")]
        public decimal Rate { get; set; }
    }

    [Serializable()]
    [System.Xml.Serialization.XmlRoot("PostageCollection")]
    public class PostageCollection
    {
        [XmlArray("Postages")]
        [XmlArrayItem("Postage", typeof(Postage))]
        public Postage[] postages { get; set; }
    }
}
