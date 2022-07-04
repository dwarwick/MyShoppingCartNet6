using MyShoppingCart.Data.Base;

namespace MyShoppingCart.Models
{
    public class ShippingMethod : IEntityBase
    {
        public int Id { get; set; }
        public Container container { get; set; }
        public ApplicationUser applicationUser { get; set; }
    }
}
