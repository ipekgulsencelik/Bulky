using Bulky.DataAccess.Repository.IRepositories;
using Bulky.Entity.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Bulky.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = ApplicationRole.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            List<Company> objCompanyList = (await _unitOfWork.Companies.GetAllAsync()).ToList();
            return View(objCompanyList);
        }

        public async Task<IActionResult> UpsertCompany(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                Company companyObj = await _unitOfWork.Companies.GetAsync(x => x.Id == id);
                return View(companyObj);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpsertCompany(Company CompanyObj)
        {
            if (ModelState.IsValid)
            {
                if (CompanyObj.Id == 0)
                {
                    await _unitOfWork.Companies.AddAsync(CompanyObj);
                }
                else
                {
                    _unitOfWork.Companies.Update(CompanyObj);
                }

                _unitOfWork.CommitAsync();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(CompanyObj);
            }
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<Company> objCompanyList = (List<Company>)await _unitOfWork.Companies.GetAllAsync();
            return Json(new { data = objCompanyList });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var CompanyToBeDeleted = await _unitOfWork.Companies.GetAsync(x => x.Id == id);
            if (CompanyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Companies.Remove(CompanyToBeDeleted);
            _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}