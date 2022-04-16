using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name ="Full name")]
        public string FullName { get; set; }
        public string Subdomain { get; set; }

        public string Domain { get; set; }
        public string newEmail { get; set; }
        public string newUserName { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public bool DomainBound { get; set; }

        public bool CertificateExists { get; set; }
        public bool CertificateBound { get; set; }
    }
}
