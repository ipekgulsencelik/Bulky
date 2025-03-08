using Bulky.DataAccess.Repository.IRepositories;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bulky.UI.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(ApplicationRole.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(ApplicationRole.SessionCart,
                    (await _unitOfWork.ShoppingCarts.GetAllAsync(x => x.ApplicationUserId == claim.Value)).Count());
                }

                return View(HttpContext.Session.GetInt32(ApplicationRole.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}