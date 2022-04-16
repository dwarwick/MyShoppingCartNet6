using MyShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Data.ViewModels
{
    public class RegisterVM
    {
        [Display(Name = "Full name")]
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; }

        [Display(Name = "Email address")]
        [Required(ErrorMessage = "Email address is required")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm password")]
        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
        
        [Display(Name = "Register as a Seller?")]
        public bool SellerAccount { get; set; } = false;

        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]        
        public string Phone { get; set; }

        [Display(Name = "Address 1")]
        [Required]        
        public string  Address1 { get; set; }

        [Display(Name = "Address 2")]        
        public string Address2 { get; set; }

        [Display(Name = "City")]
        [Required]
        public string City { get; set; }

        [Display(Name = "State")]
        [Required]
        public string State { get; set; }



        [Display(Name = "Zip / Postal Code")]
        [DataType(DataType.PostalCode)]
        [Required]
        public string ZipCode { get; set; }

        public AddressVerificationResponse addressVerificationResponse { get; set; }
    }
}
