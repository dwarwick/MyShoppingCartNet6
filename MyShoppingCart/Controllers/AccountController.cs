using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyShoppingCart.Data;
using MyShoppingCart.Data.Static;
using MyShoppingCart.Data.ViewModels;
using MyShoppingCart.Helpers;
using MyShoppingCart.Helpers.AddressValidation;
using MyShoppingCart.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyShoppingCart.Helpers.AzureManagementAPI;

namespace MyShoppingCart.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _clientFactory = clientFactory;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [AllowAnonymous]
        public IActionResult Login() => View(new LoginVM());

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);

            var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
            if (user != null)
            {
                var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
                if (passwordCheck)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, true, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Products");
                    }
                }
                TempData["Error"] = "Wrong credentials. Please, try again!";
                return View(loginVM);
            }

            TempData["Error"] = "Wrong credentials. Please, try again!";
            return View(loginVM);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Verify(string Id, string token)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(Id);

            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    if (user.newEmail != null)
                    {
                        user.newEmail = null;
                        user.newUserName = null;
                        await _userManager.UpdateAsync(user);
                        await Logout();
                    }

                    ViewBag.FullName = user.FullName;
                    return View(user);
                }

                user.Email = user.newEmail;
                user.UserName = user.newUserName;
                user.newEmail = null;
                user.newUserName = null;

                await _userManager.UpdateAsync(user);

            }

            return View();

        }
        [AllowAnonymous]
        public IActionResult Register() => View(new RegisterVM());

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            var user = await _userManager.FindByEmailAsync(registerVM.EmailAddress);
            if (user != null)
            {
                TempData["Error"] = "This email address is already in use";
                return View(registerVM);
            }

            var newUser = new ApplicationUser()
            {
                FullName = registerVM.FullName,
                Email = registerVM.EmailAddress,
                UserName = registerVM.EmailAddress,
                PhoneNumber = registerVM.Phone,
                Address1 = registerVM.Address1,
                Address2 = registerVM.Address2,
                City = registerVM.City,
                State = registerVM.State,
                Zip = registerVM.ZipCode
            };

            var addressVerification = await ValidateAddress.VerifyAddressAsync(newUser);

            if (!addressVerification.AllowAsIs)
            {
                registerVM.addressVerificationResponse = addressVerification;
                return View(registerVM);
            }
            else if (addressVerification.ErrorText != "")
            {
                registerVM.addressVerificationResponse = addressVerification;
                registerVM.Address1 = registerVM.addressVerificationResponse.rootObject.Data[0].AddressLine1;
                registerVM.Address2 = registerVM.addressVerificationResponse.rootObject.Data[0].AddressLine2;
                registerVM.City = registerVM.addressVerificationResponse.rootObject.Data[0].City;
                registerVM.State = registerVM.addressVerificationResponse.rootObject.Data[0].State;
                registerVM.ZipCode = registerVM.addressVerificationResponse.rootObject.Data[0].PostalCode;

                return View(registerVM);
            }

            var newUserResponse = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (newUserResponse.Succeeded)
            {
                if (!registerVM.SellerAccount)
                    await _userManager.AddToRoleAsync(newUser, UserRoles.User);
                else
                    await _userManager.AddToRoleAsync(newUser, UserRoles.Seller);

                await SendConfirmationEmail(newUser);

            }
            else
            {
                string error = null;
                foreach (var errorMessage in newUserResponse.Errors)
                {
                    error += errorMessage.Description + " ";
                }
                ViewBag.PasswordError = error;
                return View(registerVM);
            }



            return View("RegisterCompleted");
        }

        [Authorize]
        private async Task SendConfirmationEmail(ApplicationUser user)
        {
            string sEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string sURL = $"{_configuration.GetValue<string>("SiteURL")}Account/Verify?Id={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(sEmailToken)}";
            string sPlainTextContent = $"Please click the following link to verify your email: {sURL}";
            string sHTMLContent = $"<strong>Please click the following link to verify your email:</strong><br>{sURL}";

            await SendGridStatic.Execute(_configuration.GetValue<string>("SendGridKey"), "customerservice@myshoppingcart.biz", "Customer Service", "Please Verify Your Account", user.Email, user.FullName, sPlainTextContent, sHTMLContent);
        }


        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Products");
        }

        public IActionResult AccessDenied(string ReturnUrl)
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            EditProfileVM editProfileVM = new EditProfileVM
            {
                EmailAddress = user.Email,
                FullName = user.FullName,
                Password = user.PasswordHash,
                SellerAccount = User.IsInRole("Seller"),
                Subdomain = user.Subdomain,
                CustomDomain = user.Domain
            };

            return View(editProfileVM);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileVM editProfileVM)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            if (!User.IsInRole("Seller") && editProfileVM.SellerAccount)
            {
                return View("SellerAgreement");
            }

            else
            {
                if (!editProfileVM.SellerAccount)
                {
                    if (User.IsInRole("Seller"))
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                        await _userManager.RemoveFromRoleAsync(user, "Seller");
                        user.Subdomain = null;
                        await _userManager.UpdateAsync(user);
                        await Logout();
                        return RedirectToAction("Index", "Products");
                    }
                }
            
            }            

            return View(editProfileVM);
        }

        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> SellerAccount()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            EditProfileVM editProfileVM = new EditProfileVM
            {
                EmailAddress = user.Email,
                FullName = user.FullName,
                Password = user.PasswordHash,
                SellerAccount = User.IsInRole("Seller"),
                Subdomain = user.Subdomain,
                CustomDomain = user.Domain,
                DomainBound = user.DomainBound,
                CertificateExists = user.CertificateExists,
                CertificateBound=user.CertificateBound
            };

            return View(editProfileVM);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SellerAccount(EditProfileVM editProfileVM)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            if (!User.IsInRole("Seller") && editProfileVM.SellerAccount)
            {
                return View("SellerAgreement");
            }

            else
            {
                if (!editProfileVM.SellerAccount)
                {
                    if (User.IsInRole("Seller"))
                    {
                        var response = await AzureCustomDomain.DeleteHostName(user.Domain, user.Email);
                        await _userManager.AddToRoleAsync(user, "User");
                        await _userManager.RemoveFromRoleAsync(user, "Seller");
                        user.Subdomain = null;
                        user.Domain = null;
                        user.CertificateBound = false;
                        user.CertificateExists = false;
                        user.DomainBound = false;
                        await _userManager.UpdateAsync(user);
                        await Logout();
                        return RedirectToAction("Index", "Products");
                    }
                }
                else
                {
                    if (editProfileVM.Subdomain != null)
                        user.Subdomain = editProfileVM.Subdomain;
                    else
                        user.Subdomain = null;

                }

                await _userManager.UpdateAsync(user);
            }

            if (editProfileVM.SellerAccount && editProfileVM.CustomDomain != null && user.Domain == null)
            {
                return View("CreateCustomDomain", editProfileVM);
            }

            return View(editProfileVM);
        }



        public async Task<IActionResult> RegisterCustomDomain(string Domain)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            bool Success = await AzureCustomDomain.CreateCustomDomainAndCertificate(Domain, user.Email);

            if (Success)
            {                
                user.Domain = Domain;
                user.DomainBound = true;
                await _userManager.UpdateAsync(user); 
            }

            ViewBag.Success = Success;
            ViewBag.Domain = Domain;
            return View("CreateCustomDomainStatus");
        }

        
        [Authorize]
        public async Task<IActionResult> ChangeEmail()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            ChangeEmailVM changeEmailVM = new ChangeEmailVM
            {
                applicationUser = user
            };

            return View(changeEmailVM);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangeEmail(ChangeEmailVM changeEmailVM)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            if (user.Email != changeEmailVM.newEmail)
            {
                user.newEmail = user.Email;
                user.newUserName = user.UserName;
                user.Email = changeEmailVM.newEmail;
                user.UserName = changeEmailVM.newEmail;

                await _userManager.UpdateAsync(user);

                await SendConfirmationEmail(user);
            }

            return View("VerifyChangedEmail");
        }

        [Authorize]
        public async Task<IActionResult> AcceptSellerAgreement()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            await _userManager.RemoveFromRoleAsync(user, "User");
            await _userManager.AddToRoleAsync(user, "Seller");
            await _userManager.UpdateAsync(user);

            await Logout();

            return RedirectToAction("Index", "Products");
        }

        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> EditShippingPolicy()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            return View();
        }
    }
}
