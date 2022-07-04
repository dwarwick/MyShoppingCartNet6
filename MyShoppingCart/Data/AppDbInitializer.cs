using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyShoppingCart.Data.Static;
using MyShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Data
{
    public static class AppDbInitializer
    {
        public static void Seed(IApplicationBuilder builder)
        {
            using (var serviceScope = builder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AppDbContext>();
                context.Database.EnsureCreated();

                //Product
                if (!context.Products.Any())
                {
                    context.Products.AddRange(new List<Product>
                    {
                        new Product
                        {
                            Description = "This is the description for product 1.",
                            Name = "Product 1"
                        },

                        new Product
                        {
                            Description = "This is the description for product 2.",
                            Name = "Product 2"
                        },

                        new Product
                        {
                            Description = "This is the description for product 3.",
                            Name = "Product 3"
                        }
                    });

                    context.SaveChanges();

                    
                    
                }

                if (!context.ShippingClasses.Any())
                {
                    context.ShippingClasses.AddRange(new List<ShippingClass>
                        {
                            new ShippingClass
                            {
                                //Id = 3,
                                Name = "USPS First-Class Mail",
                                MaxWeightOz = 15.99M,
                                MaxLengthInch = 22,
                                MaxWidthInch = 18,
                                MaxHeightInch = 15,
                                MinLengthInch = 5,
                                MinWidthInch = 3.5M,
                                MinHeightInch = .007M,
                                MinMachinableLengthInch = 6,
                                MinMachinableWidthInch = 3,
                                MinMachinableHeightInch = .25M,
                                DeliveryTimeline = "1 to 5 business days (Not Guaranteed)"
                            },
                            new ShippingClass
                            {
                                //Id = 4,
                                Name = "USPS Media Mail",
                                MaxWeightOz = 1120, // 70 lbs.
                                MaxCombinedLengthAndGirth = 108,
                                DeliveryTimeline = "2 to 8 business days (Not Guaranteed)"
                            },
                            new ShippingClass
                            {
                                //Id = 3,
                                Name = "USPS Parcel Select Ground",
                                MaxWeightOz = 1120, // 70 lbs.
                                MaxCombinedLengthAndGirth = 130,
                                DeliveryTimeline = "2 to 8 business days (Not Guaranteed)"
                            },
                            new ShippingClass
                            {
                                //Id = 4,
                                Name = "USPS Priority Mail",
                                MaxWeightOz = 1120, // 70 lbs.
                                MaxCombinedLengthAndGirth = 108,
                                MaxLengthInch = 27,
                                MaxWidthInch = 17,
                                MaxHeightInch = 17,
                                DeliveryTimeline = "1 to 3 business days (Not Guaranteed)"
                            },
                            new ShippingClass
                            {
                                //Id = 4,
                                Name = "USPS Priority Mail Express",
                                MaxWeightOz = 1120, // 70 lbs.
                                MaxCombinedLengthAndGirth = 108,
                                DeliveryTimeline = "1 to 3 business days (Guaranteed). See https://www.usps.com/ship/priority-mail-express.htm for details."
                            }
                        });
                    context.SaveChanges();
                }
                //ProductImage
                //if (!context.ProductImages.Any())
                //{

                //}

            }
        }

        public static async Task SeedUsersAndRolesAsync(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {

                //Roles
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                if (!await roleManager.RoleExistsAsync(UserRoles.User))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                if (!await roleManager.RoleExistsAsync(UserRoles.Seller))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Seller));

                //Users
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                string adminUserEmail = "admin@myshoppingcart.biz";

                var adminUser = await userManager.FindByEmailAsync(adminUserEmail);
                if (adminUser == null)
                {
                    var newAdminUser = new ApplicationUser()
                    {
                        FullName = "Admin User",
                        UserName = "admin-user",
                        Email = adminUserEmail,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(newAdminUser, "NAvy__2011");
                    await userManager.AddToRoleAsync(newAdminUser, UserRoles.Admin);
                }


                string appUserEmail = "user@myshoppingcart.biz";

                var appUser = await userManager.FindByEmailAsync(appUserEmail);
                if (appUser == null)
                {
                    var newAppUser = new ApplicationUser()
                    {
                        FullName = "Application User",
                        UserName = "app-user",
                        Email = appUserEmail,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(newAppUser, "NAvy__2011");
                    await userManager.AddToRoleAsync(newAppUser, UserRoles.User);
                }
            }
        }
    }
}
