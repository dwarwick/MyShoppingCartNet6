using Microsoft.EntityFrameworkCore.Query;
using MyShoppingCart.Data.Base;
using MyShoppingCart.Data.ViewModels;
using MyShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Data.Services
{
    public interface IProductsService : IEntityBaseRepository<Product>
    {
        //Task<Movie> GetProductByIdAsync(int id);        
        Task<NewProductVM> AddNewProductAsync(NewProductVM data);
        Task<List<Product>> GetAllProductsWithImagesAsync(string Subdomain = "");
        Task<Product> GetProductByIdAsync(int Id);
        Task UpdateProductAsync(Product data);
        Task<List<ProductRating>> GetRatingsForProductAsync(int productId);
        ProductRating GetRatingForNewReview(int productId, ApplicationUser applicationUser);
        Task<ProductRating> AddNewRatingAsync(ProductRating productRating);

        Task<EditCategoriesVM> GetAllProductCategoryLookupAsync();
        Task<EditCategoriesVM> AddNewCategoryLookupAsync(int Id, string category);
    }
}
