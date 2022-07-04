using MyShoppingCart.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


namespace MyShoppingCart.Data.Cart
{
    public class ShoppingCart : Controller
    {
        public AppDbContext _context { get; set; }
        
        public string ShoppingCartId { get; set; }
        public List<ShoppingCartItem> ShoppingCartItems { get; set; }

        public ShoppingCart(AppDbContext context)
        {
            _context = context;            
        }

        public static ShoppingCart GetShoppingCart(IServiceProvider services, string cartId = "")
        {

            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;

            var httpContext = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Request.Headers;
            var context = services.GetService<AppDbContext>();



            if (session.IsAvailable)
            {
                if (cartId == "")
                    cartId = session.GetString("CartId") ?? "";
                
                session.SetString("CartId", cartId);
                session.CommitAsync();
            }
            return new ShoppingCart(context) { ShoppingCartId = cartId };
        }



        //public ShoppingCart  ClearShoppingCart(IServiceProvider services)
        //{
        //    ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
        //    var context = services.GetService<AppDbContext>();

        //    if (session.IsAvailable)
        //    {
        //        session.Remove("CartId") ;
        //        session.Clear();                
        //    }
        //    string cartId = Guid.NewGuid().ToString();
        //    return new ShoppingCart(context) { ShoppingCartId = cartId };
        //}

        public void AddItemToCart(Product product, ApplicationUser applicationUser, string shoppingCartId)
        {
            var shoppingCartItem = _context.ShoppingCartItems.FirstOrDefault(n => n.Product.Id == product.Id && n.ShoppingCartId == shoppingCartId);

            if (shoppingCartItem == null)
            {
                shoppingCartItem = new ShoppingCartItem()
                {
                    ShoppingCartId = shoppingCartId,
                    Product = product,
                    Amount = 1,                    
                    applicationUser = applicationUser
                };

                _context.ShoppingCartItems.Add(shoppingCartItem);
            }
            else
            {
                shoppingCartItem.Amount++;
            }
            _context.SaveChanges();
        }

        public void RemoveItemFromCart(Product product, IServiceProvider serviceProvider)
        {
            var shoppingCartItem = _context.ShoppingCartItems.FirstOrDefault(n => n.Product.Id == product.Id && n.ShoppingCartId == ShoppingCartId);

            if (shoppingCartItem != null)
            {
                if (shoppingCartItem.Amount > 1)
                {
                    shoppingCartItem.Amount--;
                }
                else
                {
                    _context.ShoppingCartItems.Remove(shoppingCartItem);
                }
            }
            _context.SaveChanges();
        }

        public List<ShoppingCartItem> GetShoppingCartItems()
        {

            return ShoppingCartItems ?? (ShoppingCartItems = _context.ShoppingCartItems.Where(n => n.ShoppingCartId == ShoppingCartId)
                .Include(n => n.Product)
                .Include(n => n.Product.productImages)
                .Include(n => n.applicationUser).ToList());
        }

        public decimal GetShoppingCartTotal() => _context.ShoppingCartItems.Where(n => n.ShoppingCartId == ShoppingCartId).Select(n => n.Product.Price * n.Amount).Sum();

        public List<ShippingMethod> GetContainers(List<ShoppingCartItem> shoppingCartItems) 
        {             
            return _context.ShippingMethods.Where(x => x.applicationUser == shoppingCartItems[0].applicationUser).Include(x => x.container).ToList();            
        }
        public async Task ClearShoppingCartAsync(IServiceProvider serviceProvider)
        {
            var items = await _context.ShoppingCartItems.Where(n => n.ShoppingCartId == ShoppingCartId).ToListAsync();
            _context.ShoppingCartItems.RemoveRange(items);
            await _context.SaveChangesAsync();



            ShoppingCartItems = new List<ShoppingCartItem>();
        }
    }
}
