using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class AddressVerificationResponse
    {
        public bool AllowAsIs { get; set; }
        public bool IsError { get; set; }
        public string ErrorText { get; set; }

        public RootObject rootObject { get; set; }
    }
}
