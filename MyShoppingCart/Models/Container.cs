using MyShoppingCart.Data.Base;
using System.ComponentModel.DataAnnotations;

namespace MyShoppingCart.Models
{
    public class Container : IEntityBase
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal LengthInch { get; set; }
        public decimal WidthInch { get; set; }
        public decimal HeightInch { get; set; }

        public ShippingClass shippingClass { get; set; }        
    }
}
