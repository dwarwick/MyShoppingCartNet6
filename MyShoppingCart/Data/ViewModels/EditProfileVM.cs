using MyShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Data.ViewModels
{
    public class EditProfileVM
    {
        [Display(Name = "Full Name")]
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; }

        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Email address is required")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Display(Name = "Change Password")]        
        [DataType(DataType.Password)]
        public string Password { get; set; }        
        
        [Display(Name = "Register as a Seller?")]
        public bool SellerAccount { get; set; } = false;

        [Display(Name = "Seller Subdomain (subdomain.myshoppingcart.biz)")]
        [RegularExpression("[a-z]{1,255}", ErrorMessage = "1 to 255 lowercase letters required.\n\rNo special characters or spaces.\n\rExample, if you want myselleraccount.myshoppingcart.biz, enter only myselleraccount in the textbox.")]
        [RequiredIfTrue(nameof(SellerAccount), ErrorMessage = "subdomain is required (all lowercase) 1 - 255 characters")]
        public String Subdomain { get; set; }

        [Display(Name = "Seller Custom Domain")]
        [RegularExpression("[a-z,.]{1,255}", ErrorMessage = "1 to 255 lowercase letters required.\n\rNo special characters or spaces.\n\rExample, if you own myselleraccount.com, enter myselleraccount.com in the textbox.")]
        public String CustomDomain { get; set; }

        public bool DomainBound { get; set; }
        public bool CertificateExists { get; set; }
        public bool CertificateBound { get; set; }



        [Display(Name = "Address 1")]
        [Required(ErrorMessage ="Address 1 is Required")]        
        public string Address1 { get; set; }

        [Display(Name = "Address 2")]
        public string Address2 { get; set; }

        [Display(Name = "City")]
        [Required(ErrorMessage = "City is Required")]
        public string City { get; set; }

        [Display(Name = "State")]
        [Required(ErrorMessage = "State is Required")]
        public string State { get; set; }

        [Display(Name = "Zip / Postal Code")]
        [Required(ErrorMessage = "Zip / Postal Code is Required")]
        public string Zip { get; set; }
    }
}
