using Bulky.DataAccess.Repository.IRepositories;
using Bulky.Entity.Entities;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulky.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = ApplicationRole.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> objCategoryList = (await _unitOfWork.Categories.GetAllAsync()).ToList();
            return View(objCategoryList);
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategoryAsync(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
            }

            //if (obj.Name != null && obj.Name.ToLower() == "test")
            //{
            //    ModelState.AddModelError("name", "Test is an invalid value.");
            //}

            if (ModelState.IsValid)
            {
                _unitOfWork.Categories.AddAsync(obj);
                await _unitOfWork.CommitAsync();
                TempData["success"] = "Category Created Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult EditCategory(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category? categoryFromDb = _unitOfWork.Categories.GetAsync(x => x.Id == id).Result;
            //Category? categoryFromDb1 = _db.Categories.FirstOrDefault(x => x.Id == id);
            //Category? categoryFromDb2 = _db.Categories.Where(x => x.Id == id).FirstOrDefault();

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult EditCategory(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Categories.Update(obj);
                _unitOfWork.CommitAsync();
                TempData["success"] = "Category Updated Successfully.";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult DeleteCategory(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category? categoryFromDb = _unitOfWork.Categories.GetAsync(x => x.Id == id).Result;

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("DeleteCategory")]
        public IActionResult DeletePOST(int? id)
        {
            Category? obj = _unitOfWork.Categories.GetAsync(x => x.Id == id).Result;
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Categories.Remove(obj);
            _unitOfWork.CommitAsync();
            TempData["success"] = "Category Deleted Successfully.";
            return RedirectToAction("Index");
        }
    }
}