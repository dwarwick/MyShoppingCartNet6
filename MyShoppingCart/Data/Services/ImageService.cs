using MyShoppingCart.Data.Base;
using MyShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Data.Services
{
    public class ImageService : EntityBaseRepository<ProductImage>, IImageService
    {
        private readonly AppDbContext _context;
        public ImageService(AppDbContext context) :base(context)
        {
            _context = context;
        }

        public async Task AddNewImageAsync(ProductImage data)
        {            
            await _context.ProductImages.AddAsync(data);
            await _context.SaveChangesAsync();
        }        
    }
}
