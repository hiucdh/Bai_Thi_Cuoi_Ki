using Microsoft.AspNetCore.Mvc;
using SV22T1020607.BusinessLayers;
using SV_22T1020607.Models.Common;
using SV_22T1020607.Models.Partner;

namespace SV22T1020607.Admin.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = $"{LiteCommerce.Admin.WebUserRoles.Administrator},{LiteCommerce.Admin.WebUserRoles.DataManager}")]
    public class SupplierController : Controller
    {
        private int PAGE_SIZE => Convert.ToInt32(ApplicationContext.Configuration?.GetSection("AppSettings")["PageSize"] ?? "20");

        public IActionResult Index(int page = 1, string searchValue = "")
        {
            int rowCount = PartnerDataService.CountSuppliers(searchValue);
            var data = PartnerDataService.ListOfSuppliers(page, PAGE_SIZE, searchValue);

            var model = new PaginationSearchResult<Supplier>()
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
            ViewBag.Title = "Bổ sung Nhà cung cấp";
            var model = new Supplier()
            {
                SupplierID = 0
            };
            return View("Edit", model);
        }

        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật Nhà cung cấp";
            var model = PartnerDataService.GetSupplier(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost] // Giao thức nhận dữ liệu từ Form
        public IActionResult Save(Supplier data)
        {
            // TODO: Kiểm tra xem dữ liệu đầu vào có hợp lệ hay không?
            if (string.IsNullOrWhiteSpace(data.SupplierName))
                ModelState.AddModelError(nameof(data.SupplierName), "Tên nhà cung cấp không được để trống");
            if (string.IsNullOrWhiteSpace(data.ContactName))
                ModelState.AddModelError(nameof(data.ContactName), "Tên giao dịch không được để trống");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
            if (string.IsNullOrWhiteSpace(data.Province))
                ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn Tỉnh/Thành phố");
            
            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.SupplierID == 0 ? "Bổ sung Nhà cung cấp" : "Cập nhật Nhà cung cấp";
                return View("Edit", data);
            }


            if (data.SupplierID == 0)
            {
                int id = PartnerDataService.AddSupplier(data);
                if (id <= 0)
                {
                    ModelState.AddModelError(nameof(data.Email), "Email đã tồn tại");
                    ViewBag.Title = "Bổ sung Nhà cung cấp";
                    return View("Edit", data);
                }
            }
            else
            {
                bool result = PartnerDataService.UpdateSupplier(data);
                if (!result)
                {
                    ModelState.AddModelError(nameof(data.Email), "Email đã tồn tại");
                    ViewBag.Title = "Cập nhật Nhà cung cấp";
                    return View("Edit", data);
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                PartnerDataService.DeleteSupplier(id);
                return RedirectToAction("Index");
            }

            var model = PartnerDataService.GetSupplier(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
    }
}
