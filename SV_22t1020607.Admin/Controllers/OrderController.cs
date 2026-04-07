using Microsoft.AspNetCore.Mvc;
using SV22T1020607.BusinessLayers;
using SV_22T1020607.Models.Catalog;
using SV_22T1020607.Models.Common;
using SV_22T1020607.Models.Sales;
using SV22T1020607.Admin.Models;
using SV22T1020607.Admin.AppCodes;

namespace SV22T1020607.Admin.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = $"{LiteCommerce.Admin.WebUserRoles.Administrator},{LiteCommerce.Admin.WebUserRoles.Sales}")]
    public class OrderController : Controller
    {
        private int PAGE_SIZE => Convert.ToInt32(ApplicationContext.Configuration?.GetSection("AppSettings")["PageSize"] ?? "20");
        private const int PRODUCT_PAGE_SIZE = 5;

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>("OrderSearch");
            if (input == null)
            {
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    Status = 0,
                    DateFrom = null,
                    DateTo = null
                };
            }
            return View(input);
        }

        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            input.PageSize = PAGE_SIZE;
            ApplicationContext.SetSessionData("OrderSearch", input);
            var model = await SalesDataService.ListOrdersAsync(input);
            return PartialView(model);
        }

        public IActionResult Create()
        {
            var cart = ShoppingCartService.GetShoppingCart();
            var input = ApplicationContext.GetSessionData<ProductSearchInput>("ProductSearchForSale");
            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PRODUCT_PAGE_SIZE,
                    SearchValue = ""
                };
            }
            ViewBag.ProductSearchInput = input;
            ViewBag.Cart = cart;

            // Load customers and provinces
            ViewBag.Customers = PartnerDataService.ListCustomersAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 }).Result.DataItems;
            ViewBag.Provinces = DictionaryDataService.ListProvincesAsync().Result;

            return View(cart);
        }

        public async Task<IActionResult> SearchProduct(ProductSearchInput input)
        {
            input.PageSize = PRODUCT_PAGE_SIZE;
            ApplicationContext.SetSessionData("ProductSearchForSale", input);
            var model = await CatalogDataService.ListProductsAsync(input);
            return PartialView(model);
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity, decimal salePrice)
        {
            var product = await CatalogDataService.GetProductAsync(productId);
            if (product != null)
            {
                ShoppingCartService.AddToCart(new CartItem()
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Photo = product.Photo,
                    Quantity = quantity,
                    SalePrice = salePrice
                });
            }
            return RedirectToAction("ShoppingCart");
        }

        public IActionResult RemoveFromCart(int id)
        {
            ShoppingCartService.RemoveFromCart(id);
            return RedirectToAction("ShoppingCart");
        }

        public IActionResult ClearCart()
        {
            ShoppingCartService.ClearCart();
            return RedirectToAction("ShoppingCart");
        }

        public IActionResult ShoppingCart()
        {
            return PartialView(ShoppingCartService.GetShoppingCart());
        }

        [HttpPost]
        public async Task<IActionResult> Init(int customerID = 0, string deliveryProvince = "", string deliveryAddress = "")
        {
            var cart = ShoppingCartService.GetShoppingCart();
            if (cart.Count == 0)
                return Json(new ApiResult() { Success = false, Message = "Giỏ hàng rỗng" });

            if (customerID == 0 || string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
                return Json(new ApiResult() { Success = false, Message = "Vui lòng nhập đầy đủ thông tin khách hàng và nơi giao" });

            Order data = new Order()
            {
                CustomerID = customerID,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress,
                EmployeeID = 1 // Mã nhân viên tạo đơn (Mock)
            };

            int orderID = await SalesDataService.AddOrderAsync(data);
            if (orderID > 0)
            {
                foreach (var item in cart)
                {
                    await SalesDataService.AddDetailAsync(new OrderDetail()
                    {
                        OrderID = orderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        SalePrice = item.SalePrice
                    });
                }
                ShoppingCartService.ClearCart();
                return Json(new ApiResult() { Success = true, Message = "", Data = orderID });
            }
            return Json(new ApiResult() { Success = false, Message = "Không thể lưu đơn hàng, vui lòng thử lại sau!" });
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return RedirectToAction("Index");
            ViewBag.Details = await SalesDataService.ListDetailsAsync(id);
            return View(order);
        }

        [HttpGet]
        public IActionResult Accept(int id)
        {
            return PartialView(id);
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int id, IFormCollection form)
        {
            int employeeId = 1;
            var claim = User.FindFirst("EmployeeID") ?? User.FindFirst("UserID");
            if (claim != null && int.TryParse(claim.Value, out int eid)) employeeId = eid;

            await SalesDataService.AcceptOrderAsync(id, employeeId);
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Shipping(int id)
        {
            var shippersResult = await PartnerDataService.ListShippersAsync(new PaginationSearchInput { Page = 1, PageSize = 1000 });
            ViewBag.Shippers = shippersResult.DataItems;
            return PartialView(id);
        }

        [HttpPost]
        public async Task<IActionResult> Shipping(int id, int shipperID)
        {
            if (shipperID > 0)
            {
                await SalesDataService.ShipOrderAsync(id, shipperID);
            }
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public IActionResult Finish(int id)
        {
            return PartialView(id);
        }

        [HttpPost]
        public async Task<IActionResult> Finish(int id, IFormCollection form)
        {
            await SalesDataService.CompleteOrderAsync(id);
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public IActionResult Cancel(int id)
        {
            return PartialView(id);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, IFormCollection form)
        {
            await SalesDataService.CancelOrderAsync(id);
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public IActionResult Reject(int id)
        {
            return PartialView(id);
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, IFormCollection form)
        {
            int employeeId = 1;
            var claim = User.FindFirst("EmployeeID") ?? User.FindFirst("UserID");
            if (claim != null && int.TryParse(claim.Value, out int eid)) employeeId = eid;

            await SalesDataService.RejectOrderAsync(id, employeeId);
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            return PartialView(id);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, IFormCollection form)
        {
            await SalesDataService.DeleteOrderAsync(id);
            return RedirectToAction("Index");
        }
    }
}
