using Microsoft.AspNetCore.Mvc;
using SV22T1020607.BusinessLayers;
using SV_22T1020607.Models.Common;
using SV_22T1020607.Models.Partner;
using SV22T1020607.Admin;

namespace SV22T1020607.Admin.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = $"{LiteCommerce.Admin.WebUserRoles.Administrator},{LiteCommerce.Admin.WebUserRoles.DataManager}")]
    public class CustomerController : Controller
    {
        private int PAGE_SIZE => Convert.ToInt32(ApplicationContext.Configuration?.GetSection("AppSettings")["PageSize"] ?? "20");

        /// <summary>
        /// Giao diện chính của chức năng quản lý khách hàng
        /// </summary>
        /// <returns></returns>
        private const string CUSTOMER_SEARCH = "CustomerSearch";
        public IActionResult Index()
        {
            PaginationSearchInput? input = ApplicationContext.GetSessionData<PaginationSearchInput>(CUSTOMER_SEARCH);
            if (input == null)
            {
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            }
            return View(input);
        }
        
        /// <summary>
        /// Thực hiện tìm kiếm và trả về kết quả
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            input.PageSize = PAGE_SIZE;
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH, input);
            var model = await PartnerDataService.ListCustomersAsync(input);
            return PartialView(model);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung khách hàng";
            var model = new Customer()
            {
                CustomerID = 0
            };
            return View("Edit", model);
        }

        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật khách hàng";
            var model = PartnerDataService.GetCustomer(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> SaveData(Customer data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    ModelState.AddModelError(nameof(data.CustomerName), "Tên khách hàng không được để trống");
                if (string.IsNullOrWhiteSpace(data.ContactName))
                    ModelState.AddModelError(nameof(data.ContactName), "Tên giao dịch không được để trống");
                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
                if (string.IsNullOrWhiteSpace(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh/thành");

                // Kiểm tra email trùng
                if (!string.IsNullOrWhiteSpace(data.Email))
                {
                    bool isValidEmail = await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, data.CustomerID);
                    if (!isValidEmail)
                        ModelState.AddModelError(nameof(data.Email), "Địa chỉ email đã được sử dụng");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng" : "Cập nhật khách hàng";
                    return View("Edit", data);
                }

                if (data.CustomerID == 0)
                {
                    int id = await PartnerDataService.AddCustomerAsync(data);
                    if (id <= 0)
                    {
                        ModelState.AddModelError("Error", "Không thể bổ sung khách hàng");
                        ViewBag.Title = "Bổ sung khách hàng";
                        return View("Edit", data);
                    }
                }
                else
                {
                    bool result = await PartnerDataService.UpdateCustomerAsync(data);
                    if (!result)
                    {
                        ModelState.AddModelError("Error", "Không thể cập nhật khách hàng");
                        ViewBag.Title = "Cập nhật khách hàng";
                        return View("Edit", data);
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng" : "Cập nhật khách hàng";
                return View("Edit", data);
            }
        }

        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                PartnerDataService.DeleteCustomer(id);
                return RedirectToAction("Index");
            }
            var model = PartnerDataService.GetCustomer(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        public IActionResult ChangePassword(int id = 0)
        {
            return View();
        }
    }
}
