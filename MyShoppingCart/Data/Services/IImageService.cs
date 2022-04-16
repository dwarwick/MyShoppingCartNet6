using MyShoppingCart.Data.Base;
using MyShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Data.Services
{
    public interface IImageService : IEntityBaseRepository<ProductImage>
    {
        Task AddNewImageAsync(ProductImage data);        
    }
}
