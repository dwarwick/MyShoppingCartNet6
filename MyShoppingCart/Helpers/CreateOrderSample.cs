using Microsoft.AspNetCore.Http;
using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Helpers
{
    
    public class CreateOrderSample
    {
        public static OrderRequest BuildRequestBody(string CurrencyCode, string Value, string Email)
        {
            OrderRequest orderRequest = new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                ApplicationContext = new ApplicationContext
                {

                },
                PurchaseUnits = new List<PurchaseUnitRequest>
    {
      new PurchaseUnitRequest {
        AmountWithBreakdown = new AmountWithBreakdown
        {
          CurrencyCode = CurrencyCode, // "USD",
          Value = Value, // "220.00"
        },
        Payee = new Payee
        {
          Email = Email // "payee@email.com"
        }
      }
    }
            };

            return orderRequest;
        }
    }
}
