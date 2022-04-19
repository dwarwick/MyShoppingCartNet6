using MyShoppingCart.Data;
using MyShoppingCart.Data.Services;
using MyShoppingCart.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyShoppingCart.Helpers;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MyShoppingCart.Data.Cart;
using Microsoft.AspNetCore.Authorization;
using MyShoppingCart.Data.ViewModels;

namespace MyShoppingCart.Controllers
{
    //TODO: Add Product Category GUI
    public class ProductsController : Controller
    {
        private readonly IProductsService _productService;
        private readonly IImageService _imageService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ShoppingCart _shoppingCart;
        private readonly IServiceProvider _serviceProvider;
        readonly IConfiguration _Configuration;

        IWebHostEnvironment _hostingEnvironment;

        string _ConnectionString;
        string _ContainerName;

        public ProductsController(IProductsService service, IImageService imageService, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, ShoppingCart shoppingCart, IServiceProvider serviceProvider)
        {
            _productService = service;
            _imageService = imageService;
            _Configuration = configuration;
            _hostingEnvironment = webHostEnvironment;
            _userManager = userManager;
            _shoppingCart = shoppingCart;
            _serviceProvider = serviceProvider;
        }

        public async Task<IActionResult> Index()
        {
            ReadCartIDFromCookie();

            string SubDomain = GetSubDomain(HttpContext);
            var allProducts = await _productService.GetAllProductsWithImagesAsync(SubDomain);

            var productCategoryLookup = await _productService.GetAllProductCategoryLookupAsync();

            ViewBag.host = SubDomain;
            ViewBag.productCategoryLookup = productCategoryLookup;

            return View("Index", allProducts);
        }

        public async Task<IActionResult> Filter(string searchString)
        {
            var allProducts = await _productService.GetAllProductsWithImagesAsync(GetSubDomain(HttpContext));

            if (!string.IsNullOrEmpty(searchString))
            {
                var filteredResult = allProducts.Where(n => n.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                                                        || n.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                                                        && n.Enabled == true).ToList();
                return View("Index", filteredResult);
            }

            return View("Index", allProducts);
        }

        //GET: Movies/Details/1
        public async Task<IActionResult> Details(int id)
        {
            var productDetail = await _productService.GetProductByIdAsync(id);
            var productRatings = await _productService.GetRatingsForProductAsync(id);

            ViewBag.Images = productDetail.productModel.productImages;
            ViewBag.Product = productDetail;
            ViewBag.productRatings = productRatings;
            return View();


        }

        //GET: Movies/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            if (!user.EmailConfirmed)
            {
                await SendConfirmationEmail(user);
                return View("RegisterCompleted");
            }

            if (User.IsInRole("Seller") && string.IsNullOrEmpty(user.Subdomain))
                return RedirectToAction("EditProfile", "Account");

            NewProductVM productVM = new NewProductVM
            {
                applicationUser = user
            };
            return View(productVM);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(NewProductVM productVM)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            productVM.applicationUser = user;
            //if (!ModelState.IsValid)
            //{
            //    return View(productVM);
            //}

            await _productService.AddNewProductAsync(productVM);

            Product product = await _productService.GetByIdAsync(productVM.Id);

            return View("ImagesEdit", product);
        }


        //GET: Products/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            var productDetails = await _productService.GetProductByIdAsync(id);
            if (productDetails == null) return View("NotFound");
            return View(productDetails);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditProductsVM product, int id = 0)
        {
            if (id > 0 && id != product.productModel.Id) return View("NotFound");

            EditProductsVM updatedProduct = await _productService.GetProductByIdAsync(id);

            product.productModel.applicationUser = updatedProduct.productModel.applicationUser;

            updatedProduct.productModel.Name = product.productModel.Name;
            updatedProduct.productModel.Description = product.productModel.Description;
            updatedProduct.productModel.Price = product.productModel.Price;
            updatedProduct.productModel.Enabled = product.productModel.Enabled;

			if (!ModelState.IsValid)
            {
                return View(updatedProduct);
            }
            await _productService.UpdateProductAsync(updatedProduct.productModel);
            return RedirectToAction(nameof(Edit), updatedProduct);
        }


        public async Task<IActionResult> DeleteImage(int id, int productId)
        {

            bool bSuccess = await DeleteFileFromStorage(id);

            await _imageService.DeleteAsync(id);
            EditProductsVM product = await _productService.GetProductByIdAsync(productId);

            return Redirect("/Products/EditImageDescriptions?productId=" + productId);
        }




        public IActionResult UploadFile()
        {
            NewProductVM model = new NewProductVM() { Name = "new File" };
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> UploadFile(int productId)
        {
            EditProductsVM product = await _productService.GetProductByIdAsync(productId);
            return View("ImagesEdit", product.productModel);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(int Id)
        {
            bool bSuccess = true;

            _ConnectionString = _Configuration.GetConnectionString("StorageConnectionString");
            _ContainerName = _Configuration.GetValue<string>("StorageContainerName");

            ApplicationUser user = await _userManager.GetUserAsync(User);

            string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Uploads");

            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            foreach (var iFormFile in Request.Form.Files)
            {
                if (iFormFile.Length > 0)
                {
                    if (StorageHelper.IsImage(iFormFile))
                    {
                        var filePath = Path.Combine(uploads, iFormFile.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await iFormFile.CopyToAsync(stream);
                        }

                        bSuccess = await StorageHelper.UploadFileToStorage(iFormFile, filePath, _ConnectionString, _ContainerName, user.Id);
                        System.IO.File.Delete(filePath);

                        ProductImage productImage = new ProductImage
                        {
                            ProductId = Id,
                            ImageURL = $"{_Configuration.GetValue<string>("StorageContainerURL")}/{user.Id}/{iFormFile.FileName}"
                        };

                        await _imageService.AddNewImageAsync(productImage);
                    }
                }
            }
            //RedirectToActionPermanent(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        public async Task<bool> DeleteFileFromStorage(int Id)
        {
            bool bSuccess = true;

            _ConnectionString = _Configuration.GetConnectionString("StorageConnectionString");
            _ContainerName = _Configuration.GetValue<string>("StorageContainerName");

            ProductImage image = await _imageService.GetByIdAsync(Id);

            bSuccess = await StorageHelper.DeleteFileToStorage(image.ImageURL, _ConnectionString, _ContainerName);

            return bSuccess;
        }


        private static string GetSubDomain(HttpContext httpContext)
        {
            var subDomain = string.Empty;

            var host = httpContext.Request.Host.Host;

            if (!string.IsNullOrWhiteSpace(host))
            {
                string[] pieces = host.Split('.');

                if (pieces.Length == 3)
                {
                    if (pieces[1].Equals("myshoppingcart", StringComparison.OrdinalIgnoreCase) && pieces[2].Equals("biz", StringComparison.OrdinalIgnoreCase))
                    {
                        subDomain = pieces[0];
                    }
                }

                else
                    return host;
            }

            return subDomain.Trim().ToLower();
        }

        private string SetCartIDInCookie()
        {
            var optiions = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(5)
            };

            string value = Guid.NewGuid().ToString();

            Response.Cookies.Append("CartID", value, optiions);

            _shoppingCart.ShoppingCartId = value;

            return value;
        }

        private string ReadCartIDFromCookie()
        {
            string ret = Request.Cookies["CartID"] ?? "";

            if (ret == "")
                ret = SetCartIDInCookie();

            _shoppingCart.ShoppingCartId = ret;
            Data.Cart.ShoppingCart.GetShoppingCart(_serviceProvider, ret);

            return ret;
        }

        [HttpGet]
        public async Task<IActionResult> EditImageDescriptions(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            IEnumerable<ProductImage> productImages = product.productModel.productImages;
            int productID = product.productModel.Id;

            ViewBag.productImages = productImages;
            ViewBag.productId = productID;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditImageDescriptions()
        {
            ProductImage productImage = null;
            List<string> Descriptions = new List<string>();

            foreach (var item in Request.Form["imageDescription"])
                Descriptions.Add(item);

            int idx = 0;
            foreach (var imageId in Request.Form["Id"])
            {
                productImage = await _imageService.GetByIdAsync(int.Parse(imageId));

                productImage.ImageDescription = Descriptions[idx];

                idx++;

                await _imageService.UpdateAsync(int.Parse(imageId), productImage);
            }

            int id = productImage.ProductId;
            return Redirect("/Products/Edit?id=" + id);
        }

        [HttpGet]
        public async Task<IActionResult> SelectProductCategories(int productId)
        {
            var productCategoryLookup = await _productService.GetAllProductCategoryLookupAsync();
            var product = await _productService.GetProductByIdAsync(productId);
            var productCategories = await _productService.GetProductCategoriesByIdAsync(productId);

            SelectProductCategoriesVM selectProductCategoriesVM = new SelectProductCategoriesVM
            {
                editCategoriesVM = productCategoryLookup,
                lstProductCategories = productCategories,
                product = product
            };

            return View(selectProductCategoriesVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategoryToProduct(int productId, int categoryId)
		{
            SelectProductCategoriesVM selectProductCategoriesVM = new SelectProductCategoriesVM();

            if (!await _productService.IsCategoryAssociatedWithProductAsync(productId, categoryId))
			{
                selectProductCategoriesVM = await _productService.AddProductCategoryAsync(categoryId, productId);   
			}
                            
            return RedirectToAction("SelectProductCategories", new { productId = productId});
        }

        public async Task<IActionResult> DeleteCategoryFromProduct(int categoryId, int productId)
		{
            await _productService.DeleteCategoryFromProductAsync(productId, categoryId);
            return RedirectToAction("SelectProductCategories", new { productId = productId });
        }

        [HttpGet]
        public async Task<IActionResult> EditCategories()
        {
            var productCategoryLookup = await _productService.GetAllProductCategoryLookupAsync();

            return View(productCategoryLookup);
        }


        public async Task<IActionResult> EditCategoriesPost(int Id, string category)
        {
            EditCategoriesVM editCategoriesVM = new EditCategoriesVM();

            if (category != null && Id > 0 && !string.IsNullOrWhiteSpace(category.Trim()))
                editCategoriesVM = await _productService.AddNewCategoryLookupAsync(Id, category);
            else
                return RedirectToAction("EditCategories");

            return View("EditCategories", editCategoriesVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRootCategory()
        {
            EditCategoriesVM editCategoriesVM = new EditCategoriesVM();
            string category = Request.Form["ProductCategoryLookup.CategoryName"];
            if (!string.IsNullOrWhiteSpace(category.Trim()))
                editCategoriesVM = await _productService.AddNewCategoryLookupAsync(0, category);

            return RedirectToAction("EditCategories");
        }

        [HttpGet]
        public async Task<IActionResult> WriteReview(int productId)
        {
            var productRating = _productService.GetRatingForNewReview(productId, await _userManager.GetUserAsync(User));
            return View(productRating);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitReview()
        {
            string ReviewText = Request.Form["ReviewText"];
            int rating = int.Parse(Request.Form["Rating"]);
            int id = int.Parse(Request.Form["product.Id"]);

            var productRating = _productService.GetRatingForNewReview(id, await _userManager.GetUserAsync(User));

            productRating.Rating = rating;
            productRating.ReviewText = ReviewText;

            productRating = await _productService.AddNewRatingAsync(productRating);

            //return Redirect($"Details/{id}");
            return View("ThanksForReview", id);
        }

        [Authorize]
        private async Task SendConfirmationEmail(ApplicationUser user)
        {
            string sEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string sURL = $"{_Configuration.GetValue<string>("SiteURL")}Account/Verify?Id={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(sEmailToken)}";
            string sPlainTextContent = $"Please click the following link to verify your email: {sURL}";
            string sHTMLContent = $"<strong>Please click the following link to verify your email:</strong><br>{sURL}";

            await SendGridStatic.Execute(_Configuration.GetValue<string>("SendGridKey"), "customerservice@myshoppingcart.biz", "Customer Service", "Please Verify Your Account", user.Email, user.FullName, sPlainTextContent, sHTMLContent);
        }
    }
}