using Bulky.DataAccess.Repository.IRepositories;
using Bulky.Entity.Entities;
using Bulky.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bulky.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> objProductList = (await _unitOfWork.Products.GetAllAsync(includeProperties: "Category")).ToList();
            return View(objProductList);
        }

        public async Task<IActionResult> UpsertProduct(int? id)
        {
            ProductViewModel productViewModel = new()
            {
                CategoryList = (await _unitOfWork.Categories.GetAllAsync()).Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                //create
                return View(productViewModel);
            }
            else
            {
                //update
                productViewModel.Product = await _unitOfWork.Products.GetAsync(x => x.Id == id);
                return View(productViewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpsertProduct(ProductViewModel productViewModel, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\products");

                    if (!string.IsNullOrEmpty(productViewModel.Product.ImageUrl))
                    {
                        //delete the old image
                        var oldImagePath =
                            Path.Combine(wwwRootPath, productViewModel.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productViewModel.Product.ImageUrl = @"\images\products\" + fileName;
                }

                if (productViewModel.Product.Id == 0)
                {
                    await _unitOfWork.Products.AddAsync(productViewModel.Product);
                }
                else
                {
                    _unitOfWork.Products.Update(productViewModel.Product);
                }

                await _unitOfWork.CommitAsync();
                TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productViewModel.CategoryList = (await _unitOfWork.Categories.GetAllAsync()).Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
              
                return View(productViewModel);
            }
        }

        public IActionResult DeleteProduct(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? productFromDb = _unitOfWork.Products.GetAsync(x => x.Id == id).Result;

            if (productFromDb == null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }

        [HttpPost, ActionName("DeleteProduct")]
        public IActionResult DeletePOST(int? id)
        {
            Product? obj = _unitOfWork.Products.GetAsync(x => x.Id == id).Result;
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Products.Remove(obj);
            _unitOfWork.CommitAsync();
            TempData["success"] = "Product Deleted Successfully.";
            return RedirectToAction("Index");
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<Product> objProductList = (List<Product>)await _unitOfWork.Products.GetAllAsync(includeProperties: "Category");
            return Json(new { data = objProductList });
        }

        #endregion
    }
}