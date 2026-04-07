using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020607.BusinessLayers;
using SV_22T1020607.Models.Sales;
using SV22T1020607.Shop.AppCodes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020607.Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private int? GetCurrentCustomerID()
        {
            var claim = User.FindFirst("CustomerID");
            if (claim != null && int.TryParse(claim.Value, out int id)) return id;
            var nameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (nameClaim != null && int.TryParse(nameClaim.Value, out int nId)) return nId;
            return null;
        }

        public async Task<IActionResult> Init()
        {
            var cart = HttpContext.GetCart();
            if (cart == null || cart.Count == 0)
                return RedirectToAction("Index", "Cart");
            
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Init(string deliveryProvince, string deliveryAddress)
        {
            var cart = HttpContext.GetCart();
            if (cart == null || cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            if (string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
            {
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                ModelState.AddModelError("Error", "Vui lòng nhập Tỉnh/Thành và Địa chỉ giao hàng.");
                return View(cart);
            }

            int? customerId = GetCurrentCustomerID();
            if(!customerId.HasValue) 
                return RedirectToAction("Login", "Account");

            var order = new Order
            {
                CustomerID = customerId.Value,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress
            };

            int orderId = await SalesDataService.AddOrderAsync(order);

            foreach (var item in cart)
            {
                await SalesDataService.AddDetailAsync(new OrderDetail
                {
                    OrderID = orderId,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                });
            }

            HttpContext.ClearCart();
            return RedirectToAction("Details", new { id = orderId });
        }

        public async Task<IActionResult> Index()
        {
            int? customerId = GetCurrentCustomerID();
            if (!customerId.HasValue) return View(); // empty

            // Since we don't have a direct method to filter by Customer in SalesDataService,
            // we will fetch all and filter, or we will modify the input / wait.
            // Oh, OrderSearchInput has no CustomerID ? Wait, OrderSearchInput inherits PaginationSearchInput.
            // Let's just fetch by searchValue for now, wait, no. Admin can search.
            // But how do we get orders for exclusively a customer?
            var input = new OrderSearchInput { Page = 1, PageSize = 1000, SearchValue = "" };
            var result = await SalesDataService.ListOrdersAsync(input);
            var myOrders = result.DataItems.Where(o => o.CustomerID == customerId.Value).ToList();
            
            return View(myOrders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null || order.CustomerID != GetCurrentCustomerID())
                return RedirectToAction("Index");

            var details = await SalesDataService.ListDetailsAsync(id);
            ViewBag.Details = details;

            return View(order);
        }
    }
}
