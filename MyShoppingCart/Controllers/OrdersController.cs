using MyShoppingCart.Data.Cart;
using MyShoppingCart.Data.Services;
using MyShoppingCart.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MyShoppingCart.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using MyShoppingCart.Helpers;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using MyShoppingCart.Helpers.Paypal;

namespace MyShoppingCart.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IProductsService _productService;
        private readonly ShoppingCart _shoppingCart;
        private readonly IOrdersService _ordersService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public OrdersController(IProductsService productService, ShoppingCart shoppingCart, IOrdersService ordersService, UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _productService = productService;
            _shoppingCart = shoppingCart;
            _ordersService = ordersService;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ApplicationUser applicationUser = await _userManager.GetUserAsync(User);
            string userId = applicationUser.Id;

            var orders = await _ordersService.GetOrdersByUserIdAsync(userId);
            ViewBag.FullName = applicationUser.FullName;
            return View(orders);
        }

        [Authorize]
        public IActionResult ShoppingCart()
        {
            var items = _shoppingCart.GetShoppingCartItems();
            _shoppingCart.ShoppingCartItems = items;

            var response = new ShoppingCartVM()
            {
                ShoppingCart = _shoppingCart,
                ShoppingCartTotal = _shoppingCart.GetShoppingCartTotal()
            };

            if (response.ShoppingCart.ShoppingCartItems.Count > 0)
                return View(response);
            else
                return Redirect("/Products");
        }

        [Authorize]
        public async Task<IActionResult> AddItemToShoppingCart(int id)
        {
            var item = await _productService.GetProductByIdAsync(id);
            ApplicationUser applicationUser = await _userManager.GetUserAsync(User);

            if (!applicationUser.EmailConfirmed)
            {
                await SendConfirmationEmail(applicationUser);
                return View("RegisterCompleted");
            }

            if (item != null)
            {
                _shoppingCart.AddItemToCart(item.productModel, applicationUser, ReadCartIDFromCookie());
            }
            return RedirectToAction(nameof(ShoppingCart));
        }

        [Authorize]
        public async Task<IActionResult> RemoveItemFromShoppingCart(int id)
        {
            var item = await _productService.GetProductByIdAsync(id);

            if (item != null)
            {
                _shoppingCart.RemoveItemFromCart(item.productModel, _serviceProvider);
            }
            return RedirectToAction(nameof(ShoppingCart));
        }

        [Authorize]
        public async Task<IActionResult> CompleteOrder()
        {
            var items = _shoppingCart.GetShoppingCartItems();


            Models.Order order = await _ordersService.StoreOrderAsync(items);

            PrepareSellerEmail(items, order, "You Have a New Order!");
            PrepareBuyerEmail(items, order, "Thank You for Your Order!");
            await _shoppingCart.ClearShoppingCartAsync(_serviceProvider);
            DeleteCartIDCookie();

            
            

            return View("OrderCompleted");
        }

        [Authorize]
        private async Task SendSellerEmail(ApplicationUser user, string sHTMLContent, string Subject)
        {
            string sPlainTextContent = "";


            await SendGridStatic.Execute(_configuration.GetValue<string>("SendGridKey"), "customerservice@myshoppingcart.biz", "Customer Service", Subject, user.Email, user.FullName, sPlainTextContent, sHTMLContent);
        }

        private async void PrepareSellerEmail(List<ShoppingCartItem> shoppingCartItems, Models.Order order, string subject)
        {
            List<ApplicationUser> Sellers = GetListOfShoppingCartSellers(shoppingCartItems).Distinct().ToList();

            foreach (ApplicationUser seller in Sellers)
            {
                List<ShoppingCartItem> items = GetShoppingCartItemsForSeller(seller, shoppingCartItems);

                string imgSource = $"{_configuration.GetValue<string>("StorageContainerURL")}/site-files/store-logo.png";

                string sHTMLContent = $"<img src='{imgSource}' style='max-height:75px;width:auto' alt='store logo' />";
                sHTMLContent += $"<br><br><h3>Congratulations {seller.FullName}!</h3>";
                sHTMLContent += $"<strong>{items[0].applicationUser.FullName} just placed an order! Here is the address and contact details:</strong><br><br>";
                sHTMLContent += $"Order Number: {order.Id}<br><br>";
                sHTMLContent += $"Order Date: {order.OrderDate.ToLocalTime().ToShortDateString()}<br>";
                sHTMLContent += $"<p>Email: {items[0].applicationUser.Email}<br>";
                sHTMLContent += $"Phone: {items[0].applicationUser.PhoneNumber}<br><br>";
                sHTMLContent += $"Address1: {items[0].applicationUser.Address1}<br>";
                sHTMLContent += $"Address2: {items[0].applicationUser.Address2}<br>";
                sHTMLContent += $"City: {items[0].applicationUser.City}<br>";
                sHTMLContent += $"State: {items[0].applicationUser.State}<br>";
                sHTMLContent += $"Zip: {items[0].applicationUser.Zip}<br><br><br><br></p>";
                sHTMLContent += $"<h3>Items Ordered</h3>";

                sHTMLContent += "<table style = 'Width:600px;border: 1px solid black;'>";
                sHTMLContent += "<thead>";
                sHTMLContent += "<tr>";

                sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Product Image</th>";
                sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Selected amount</th>";
                sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Product</th>";
                sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Price</th>";
                sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Subtotal</th>";

                sHTMLContent += "</tr>";
                sHTMLContent += "</thead>";

                sHTMLContent += "<tbody>";

                decimal total = 0;

                foreach (ShoppingCartItem item in items)
                {
                    sHTMLContent += "<tr>";

                    sHTMLContent += $"<img src='{item.Product.productImages[0].ImageURL}' style='max-height:75px;width:auto' alt='{item.Product.productImages[0].ImageDescription}' />";
                    sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Amount}</td>";
                    sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Product.Name}</td>";
                    sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Product.Price.ToString("c")}</td>";
                    sHTMLContent += $"<td style=';border: 1px solid black;'>{((item.Amount * item.Product.Price).ToString("c"))}</td>";
                    sHTMLContent += "</tr>";

                    total += (item.Amount * item.Product.Price);
                }

                sHTMLContent += "</tbody>";

                sHTMLContent += "</table>";

                sHTMLContent += $"<br><br>Total: {total.ToString("c")}";

                await SendSellerEmail(seller, sHTMLContent, subject);
            }

        }

        private async void PrepareBuyerEmail(List<ShoppingCartItem> shoppingCartItems, Models.Order order, string subject)
        {
            List<ApplicationUser> Sellers = GetListOfShoppingCartSellers(shoppingCartItems).Distinct().ToList();


            string imgSource = $"{_configuration.GetValue<string>("StorageContainerURL")}/site-files/store-logo.png";

            string sHTMLContent = $"<img src='{imgSource}' style='max-height:75px;width:auto' alt='store logo' />";

            sHTMLContent += $"<br><br><strong>{shoppingCartItems[0].applicationUser.FullName}, thank you for placing an order with us! Here are the order details:</strong><br><br>";
            sHTMLContent += $"Order Number: {order.Id}<br><br>";
            sHTMLContent += $"Order Date: {order.OrderDate.ToLocalTime().ToShortDateString()}<br>";

            sHTMLContent += $"Shipping Address<br><br>";
            sHTMLContent += $"Address1: {shoppingCartItems[0].applicationUser.Address1}<br>";
            sHTMLContent += $"Address2: {shoppingCartItems[0].applicationUser.Address2}<br>";
            sHTMLContent += $"City: {shoppingCartItems[0].applicationUser.City}<br>";
            sHTMLContent += $"State: {shoppingCartItems[0].applicationUser.State}<br>";
            sHTMLContent += $"Zip: {shoppingCartItems[0].applicationUser.Zip}<br><br><br><br></p>";
            sHTMLContent += $"<h3>Items Ordered</h3>";

            sHTMLContent += "<table style = 'Width:600px;border: 1px solid black;'>";
            sHTMLContent += "<thead>";
            sHTMLContent += "<tr>";

            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Product Image</th>";
            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Seller Email</th>";

            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Quantity</th>";
            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Product</th>";
            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Price</th>";
            sHTMLContent += "<th style='text-align:center;border: 1px solid black;'>Subtotal</th>";

            sHTMLContent += "</tr>";
            sHTMLContent += "</thead>";

            sHTMLContent += "</tbody>";

            decimal total = 0;

            foreach (ShoppingCartItem item in shoppingCartItems)
            {
                string productImgSource = item.Product.productImages[0].ImageURL;

                sHTMLContent += "<tr>";

                sHTMLContent += $"<td><img src='{productImgSource}' style='max-height:75px;width:auto' alt='{item.Product.productImages[0].ImageDescription}' /></td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Product.applicationUser.Email}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Amount}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Product.Name}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{item.Product.Price.ToString("c")}</td>";
                sHTMLContent += $"<td style=';border: 1px solid black;'>{((item.Amount * item.Product.Price).ToString("c"))}</td>";
                sHTMLContent += "</tr>";

                total += (item.Amount * item.Product.Price);
            }

            sHTMLContent += "</tbody>";

            sHTMLContent += "</table>";

            sHTMLContent += $"<br><br>Total: {total.ToString("c")}";

            await SendSellerEmail(shoppingCartItems[0].applicationUser, sHTMLContent, subject);


        }

        private List<ApplicationUser> GetListOfShoppingCartSellers(List<ShoppingCartItem> shoppingCartItems)
        {
            return _ordersService.GetSellersFromShoppingCartItems(shoppingCartItems);
        }

        private List<ShoppingCartItem> GetShoppingCartItemsForSeller(ApplicationUser seller, List<ShoppingCartItem> shoppingCartItems)
        {
            return _ordersService.GetShoppingCartItemsForSeller(seller, shoppingCartItems);
        }

        private string SetCartIDInCookie()
        {
            var optiions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(365)
            };

            string value = Guid.NewGuid().ToString();

            Response.Cookies.Append("CartID", value, optiions);

            _shoppingCart.ShoppingCartId = value;

            return value;
        }

        private string ReadCartIDFromCookie()
        {
            string ret = Request.Cookies["CartID"] ?? "";

            if (ret == "")
                ret = SetCartIDInCookie();

            _shoppingCart.ShoppingCartId = ret;
            Data.Cart.ShoppingCart.GetShoppingCart(_serviceProvider, ret);

            return ret;
        }

        private void DeleteCartIDCookie()
        {
            Response.Cookies.Delete("CartID");
        }

        private async Task SendConfirmationEmail(ApplicationUser user)
        {
            string sEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string sURL = $"{_configuration.GetValue<string>("SiteURL")}Account/Verify?Id={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(sEmailToken)}";
            string sPlainTextContent = $"Please click the following link to verify your email: {sURL}";
            string sHTMLContent = $"<strong>Please click the following link to verify your email:</strong><br>{sURL}";

            await SendGridStatic.Execute(_configuration.GetValue<string>("SendGridKey"), "customerservice@myshoppingcart.biz", "Customer Service", "Please Verify Your Account", user.Email, user.FullName, sPlainTextContent, sHTMLContent);
        }




        /// <summary>
        /// This action is called when the user clicks on the PayPal button.
        /// </summary>
        /// <returns></returns>
        [Route("Orders/create")]
        public async Task<SmartButtonHttpResponse> Create()
        {
            var items = _shoppingCart.GetShoppingCartItems();
            _shoppingCart.ShoppingCartItems = items;

            var cartVM = new ShoppingCartVM()
            {
                ShoppingCart = _shoppingCart,
                ShoppingCartTotal = _shoppingCart.GetShoppingCartTotal(),
                ShippingMethods = _shoppingCart.GetContainers(items)
            };

            var request = new OrdersCreateRequest();

            request.Prefer("return=representation");
            request.RequestBody(await OrderBuilder.Build(cartVM));

            // Call PayPal to set up a transaction
            var response = await PayPalClient.Client().Execute(request);

            // Create a response, with an order id.
            var result = response.Result<PayPalCheckoutSdk.Orders.Order>();
            var payPalHttpResponse = new SmartButtonHttpResponse(response)
            {
                orderID = result.Id
            };

            

            return payPalHttpResponse;
        }

        /// <summary>
        /// This action is called once the PayPal transaction is approved
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [Route("Orders/approved/{orderId}")]
        public IActionResult Approved(string orderId)
        {
            return Ok();
        }

        /// <summary>
        /// This action is called once the PayPal transaction is complete
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [Route("Orders/complete/{orderId}")]
        public async Task<IActionResult> Complete(string orderId)
        {
            // 1. Update the database.
            // 2. Complete the order process. Create and send invoices etc.
            // 3. Complete the shipping process.

            var items = _shoppingCart.GetShoppingCartItems();
            Models.Order order = await _ordersService.StoreOrderAsync(items);

            PrepareSellerEmail(items, order, "You Have a New Order!");
            PrepareBuyerEmail(items, order, "Thank You for Your Order!");
            await _shoppingCart.ClearShoppingCartAsync(_serviceProvider);
            DeleteCartIDCookie();

            return View("OrderCompleted");

            //return Ok();
        }

        /// <summary>
        /// This action is called once the PayPal transaction is complete
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [Route("Orders/cancel/{orderId}")]
        public IActionResult Cancel(string orderId)
        {
            // 1. Remove the orderId from the database.
            return Ok();
        }

        /// <summary>
        /// This action is called once the PayPal transaction is complete
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [Route("Orders/error/{orderId}/{error}")]
        public IActionResult Error(string orderId,
                                   string error)
        {
            // Log the error.
            // Notify the user.
            return NoContent();
        }
    }
}
