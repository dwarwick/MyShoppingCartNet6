using MyShoppingCart.Models;

namespace MyShoppingCart.Data.ViewModels
{
    public class EditProductsVM
    {
		public Product product { get; set; }
		public EditCategoriesVM editCategoriesVM { get; set; }
	}
}
