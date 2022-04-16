using MyShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Data.ViewModels
{
    public class ChangeEmailVM
    {
        public ApplicationUser applicationUser { get; set; }
        
        [DataType(DataType.EmailAddress)]
        public string newEmail { get; set; }
    }
}
