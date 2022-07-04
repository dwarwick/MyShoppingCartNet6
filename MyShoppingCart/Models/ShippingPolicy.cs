using MyShoppingCart.Data.Base;
using System.ComponentModel.DataAnnotations;

namespace MyShoppingCart.Models
{
    public class ShippingPolicy : IEntityBase
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Seller Id - This is this sellers shipping policy
        /// </summary>
        public ApplicationUser ApplicationUser { get; set; } 
        public ShippingClass ShippingClass { get; set; }
        public string Criteria { get; set; }
    }
}
