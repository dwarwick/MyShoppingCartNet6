﻿using MyShoppingCart.Data.Base;
using MyShoppingCart.Data.ViewModels;
using MyShoppingCart.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.AspNetCore.Http;
using MyShoppingCart.Helpers;

namespace MyShoppingCart.Data.Services
{
    public class ProductsService : EntityBaseRepository<Product>, IProductsService
    {
        private readonly AppDbContext _context;
        public ProductsService(AppDbContext context) : base(context)
        {
            _context = context;
        }

		public async Task<EditCategoriesVM> AddNewCategoryLookupAsync(int Id, string category)
		{
			var ProductCategoryLookup = new ProductCategoryLookup 
            { 
                CategoryName = category,
                ParentCategoryId = Id                
            };
            await _context.ProductCategoryLookups.AddAsync(ProductCategoryLookup);
            await _context.SaveChangesAsync();

            return await GetAllProductCategoryLookupAsync();
		}

		

		public async Task<NewProductVM> AddNewProductAsync(NewProductVM data)
        {
            var newProduct = new Product()
            {
                Name = data.Name,
                Description = data.Description,
                Price = data.Price,
                Enabled = true,
                applicationUser = data.applicationUser

            };
            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();
            data.Id = newProduct.Id;
            return data;
        }

        public async Task<ProductRating> AddNewRatingAsync(ProductRating productRating)
        {
            await _context.ProductRatings.AddAsync(productRating);
            await _context.SaveChangesAsync();

            var product = await _context.Products.Include(x => x.productImages)
                .FirstOrDefaultAsync(x => x.Id == productRating.product.Id);

            var productRatings = _context.ProductRatings.Where(x => x.product.Id == product.Id);

            double newRating = productRatings.Average(x => x.Rating);
            product.Rating = newRating;
            product.NumberOfReviews = productRatings.Count();
            _context.SaveChanges();







            return productRating;
        }

        public async Task<EditCategoriesVM> GetAllProductCategoryLookupAsync()
        {
            EditCategoriesVM editCategoriesVM = new EditCategoriesVM 
            { 
                lstProductCategoryLookup = await _context.ProductCategoryLookups.ToListAsync() 
            };
            return editCategoriesVM;
        }

        public async Task<List<Product>> GetAllProductsWithImagesAsync(string Subdomain = "")
        {
            List<Product> ProductDetails = null;

            if (Subdomain == "" || Subdomain == "localhost" || Subdomain.Equals("myshoppingcart.biz", StringComparison.OrdinalIgnoreCase))
                ProductDetails = await _context.Products
                    .Include(c => c.productImages).Where(c => c.productImages.Count > 0 && c.Enabled == true).ToListAsync();
            else
                ProductDetails = await _context.Products
                    .Include(c => c.productImages).Where(c => c.productImages.Count > 0 && c.Enabled == true && (c.applicationUser.Subdomain == Subdomain || c.applicationUser.Domain == Subdomain)).ToListAsync();

            return ProductDetails;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            var ProductDetails = await _context.Products
                .Include(c => c.productImages)
                .FirstOrDefaultAsync(n => n.Id == id);
            return ProductDetails;
        }

        public ProductRating GetRatingForNewReview(int productId, ApplicationUser applicationUser)
        {
            ProductRating productRating = new ProductRating
            {
                applicationUser = applicationUser,
                Date = DateTime.Now,
                product = _context.Products.Find(productId)
            };
            return productRating;
        }

        public async Task<List<ProductRating>> GetRatingsForProductAsync(int productId) => await _context.ProductRatings.Where(x => x.product.Id == productId)
            .Include(x => x.product)
            .Include(x => x.applicationUser)
            .OrderBy(x => x.Date).ToListAsync();
        

        public async Task UpdateProductAsync(Product data)
        {
            data = await _context.Products.FirstOrDefaultAsync(n => n.Id == data.Id);

            await _context.SaveChangesAsync();
        }
    }
}
