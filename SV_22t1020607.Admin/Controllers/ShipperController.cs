using Microsoft.AspNetCore.Mvc;
using SV_22T1020607.Models.Common;

namespace SV22T1020607.Admin.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = $"{LiteCommerce.Admin.WebUserRoles.Administrator},{LiteCommerce.Admin.WebUserRoles.DataManager}")]
    public class ShipperController : Controller
    {
        private int PAGE_SIZE => Convert.ToInt32(ApplicationContext.Configuration?.GetSection("AppSettings")["PageSize"] ?? "20");
        private const string SHIPPER_SEARCH = "ShipperSearch";

        public IActionResult Index()
        {
            PaginationSearchInput? input = ApplicationContext.GetSessionData<PaginationSearchInput>(SHIPPER_SEARCH);
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

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            input.PageSize = PAGE_SIZE;
            ApplicationContext.SetSessionData(SHIPPER_SEARCH, input);
            var model = await BusinessLayers.PartnerDataService.ListShippersAsync(input);
            return PartialView(model);
        }
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Edit()
        {
            return View();
        }
        public IActionResult Delete()
        {
            return View();
        }
    }
}
