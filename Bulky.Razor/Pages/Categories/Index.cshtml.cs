using Bulky.Razor.Data;
using Bulky.Razor.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky.Razor.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly BulkyContext _db;

        public List<Category> CategoryList { get; set; }

        public IndexModel(BulkyContext db)
        {
            _db = db;
        }

        public void OnGet()
        {
            CategoryList = _db.Categories.ToList();
        }
    }
}