using Microsoft.AspNetCore.Mvc;
using SV22T1020607.BusinessLayers;

namespace SV22T1020607.Shop.Controllers
{
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 12;

        public IActionResult Index(int page = 1, string searchValue = "", int categoryID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            searchValue = searchValue ?? "";
            var data = ProductDataService.ListProducts(page, PAGE_SIZE, searchValue, categoryID, 0, minPrice, maxPrice);
            int rowCount = ProductDataService.CountProducts(searchValue, categoryID, 0, minPrice, maxPrice);

            ViewBag.SearchValue = searchValue;
            ViewBag.CategoryID = categoryID;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Page = page;
            ViewBag.RowCount = rowCount;
            ViewBag.PageCount = rowCount / PAGE_SIZE + (rowCount % PAGE_SIZE > 0 ? 1 : 0);

            return View(data);
        }

        public IActionResult Details(int id)
        {
            var product = ProductDataService.GetProduct(id);
            if (product == null) return RedirectToAction("Index");

            var photos = ProductDataService.ListPhotos(id);
            var attributes = ProductDataService.ListAttributes(id);

            ViewBag.Photos = photos;
            ViewBag.Attributes = attributes;

            // Lấy tên loại hàng
            if (product.CategoryID.HasValue && product.CategoryID > 0)
            {
                var category = CommonDataService.ListOfCategories()
                    .FirstOrDefault(c => c.CategoryID == product.CategoryID);
                ViewBag.CategoryName = category?.CategoryName ?? "";
            }

            // Lấy tên nhà cung cấp
            if (product.SupplierID.HasValue && product.SupplierID > 0)
            {
                var supplier = PartnerDataService.ListOfSuppliers()
                    .FirstOrDefault(s => s.SupplierID == product.SupplierID);
                ViewBag.SupplierName = supplier?.SupplierName ?? "";
            }

            return View(product);
        }
    }
}
