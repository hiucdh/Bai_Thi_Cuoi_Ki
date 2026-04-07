using Microsoft.AspNetCore.Mvc;
using SV22T1020607.BusinessLayers;
using SV_22T1020607.Models.Common;
using SV_22T1020607.Models.Catalog;

namespace SV22T1020607.Admin.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = $"{LiteCommerce.Admin.WebUserRoles.Administrator},{LiteCommerce.Admin.WebUserRoles.DataManager}")]
    public class CategoryController : Controller
    {
        private int PAGE_SIZE => Convert.ToInt32(ApplicationContext.Configuration?.GetSection("AppSettings")["PageSize"] ?? "20");

        public IActionResult Index(int page = 1, string searchValue = "")
        {
            int rowCount = CommonDataService.CountCategories(searchValue);
            var data = CommonDataService.ListOfCategories(page, PAGE_SIZE, searchValue);

            var model = new PaginationSearchResult<Category>()
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue,
                RowCount = rowCount,
                Data = data
            };

            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung Loại hàng";
            var model = new Category()
            {
                CategoryID = 0
            };
            return View("Edit", model);
        }

        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật Loại hàng";
            var model = CommonDataService.GetCategory(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public IActionResult Save(Category data)
        {
            if (string.IsNullOrWhiteSpace(data.CategoryName))
                ModelState.AddModelError(nameof(data.CategoryName), "Tên loại hàng không được để trống");
            
            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.CategoryID == 0 ? "Bổ sung Loại hàng" : "Cập nhật Loại hàng";
                return View("Edit", data);
            }

            if (data.CategoryID == 0)
            {
                int id = CommonDataService.AddCategory(data);
                if (id <= 0)
                {
                    ModelState.AddModelError(nameof(data.CategoryName), "Tên loại hàng đã tồn tại");
                    ViewBag.Title = "Bổ sung Loại hàng";
                    return View("Edit", data);
                }
            }
            else
            {
                bool result = CommonDataService.UpdateCategory(data);
                if (!result)
                {
                    ModelState.AddModelError(nameof(data.CategoryName), "Tên loại hàng đã tồn tại");
                    ViewBag.Title = "Cập nhật Loại hàng";
                    return View("Edit", data);
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                CommonDataService.DeleteCategory(id);
                return RedirectToAction("Index");
            }

            var model = CommonDataService.GetCategory(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
    }
}
