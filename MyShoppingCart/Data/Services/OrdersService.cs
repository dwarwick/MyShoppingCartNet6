using MyShoppingCart.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Data.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly AppDbContext _context;
        public OrdersService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            var orders = await _context.Orders.Include(n => n.OrderItems).ThenInclude(n => n.Product).Where(n => n.UserId == userId).OrderBy(n => n.OrderDate).ToListAsync();
            return orders;
        }

        public List<ApplicationUser> GetSellersFromShoppingCartItems(List<ShoppingCartItem> shoppingCartItems) => shoppingCartItems.Select(x => x.Product.applicationUser).ToList();

        public List<ShoppingCartItem> GetShoppingCartItemsForSeller(ApplicationUser seller, List<ShoppingCartItem> shoppingCartItems) => shoppingCartItems.FindAll(x => x.Product.applicationUser == seller);

        public async Task<List<OrderItem>> GetUnpaidOrderItemsForSeller(ApplicationUser seller) => await _context.OrderItems.Where(x => x.Product.applicationUser == seller && x.payout != null).ToListAsync();
        
        public async Task<Order> StoreOrderAsync(List<ShoppingCartItem> items)
        {
            var order = new Order()
            {
                UserId = items[0].applicationUser.Id,
                Email = items[0].applicationUser.Email,
                OrderDate = DateTime.Now
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                var orderItem = new OrderItem()
                {
                    Amount = item.Amount,
                    ProductId = item.Product.Id,
                    OrderId = order.Id,
                    Price = item.Product.Price
                };
                await _context.OrderItems.AddAsync(orderItem);
            }
            await _context.SaveChangesAsync();

            return order;
        }
    }
}
