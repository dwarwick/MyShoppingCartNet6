using MyShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Data.Services
{
    public interface IOrdersService
    {
        Task<Order> StoreOrderAsync(List<ShoppingCartItem> items);
        Task<List<Order>> GetOrdersByUserIdAsync(string userId);

        List<ApplicationUser> GetSellersFromShoppingCartItems(List<ShoppingCartItem> shoppingCartItems);
        List<ShoppingCartItem> GetShoppingCartItemsForSeller(ApplicationUser seller, List<ShoppingCartItem> shoppingCartItems);
        Task<List<OrderItem>> GetUnpaidOrderItemsForSeller(ApplicationUser seller);
    }
}
