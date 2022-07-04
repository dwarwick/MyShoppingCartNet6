using MyShoppingCart.Data.Base;
using System.ComponentModel.DataAnnotations;

namespace MyShoppingCart.Models
{
    public class ShippingClass :IEntityBase
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal MaxWeightOz { get; set; }
        public decimal MaxLengthInch { get; set; }
        public decimal MaxWidthInch { get; set; }
        public decimal MaxHeightInch { get; set; }

        
        public decimal MinLengthInch { get; set; }
        public decimal MinWidthInch { get; set; }
        public decimal MinHeightInch { get; set; }

        public decimal MinMachinableLengthInch { get; set; }
        public decimal MinMachinableWidthInch { get; set; }
        public decimal MinMachinableHeightInch { get; set; }
        
        public decimal MaxCombinedLengthAndGirth { get; set; }

        public string DeliveryTimeline { get; set; }
        
    }
}
