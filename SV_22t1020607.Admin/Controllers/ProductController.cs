using Microsoft.AspNetCore.Mvc;
using SV22T1020607.BusinessLayers;
using SV_22T1020607.Models.Catalog;
using SV_22T1020607.Models.Common;
using System;

namespace SV22T1020607.Admin.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = $"{LiteCommerce.Admin.WebUserRoles.Administrator},{LiteCommerce.Admin.WebUserRoles.DataManager}")]
    public class ProductController : Controller
    {
        private int PAGE_SIZE => Convert.ToInt32(ApplicationContext.Configuration?.GetSection("AppSettings")["PageSize"] ?? "20");
        private const string PRODUCT_SEARCH_SESSION = "ProductSearchSession";

        public IActionResult Index()
        {
            ProductSearchInput? input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_SESSION);
            
            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };
            }

            return View(input);
        }

        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            input.PageSize = PAGE_SIZE;
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_SESSION, input);
            var model = await CatalogDataService.ListProductsAsync(input);
            return PartialView(model);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            ViewBag.IsEdit = false;
            var model = new Product()
            {
                ProductID = 0,
                Photo = "nophoto.png"
            };
            return View("Edit", model);
        }

        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật mặt hàng";
            ViewBag.IsEdit = true;
            var model = ProductDataService.GetProduct(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public IActionResult Save(Product data)
        {
            if (string.IsNullOrWhiteSpace(data.ProductName))
                ModelState.AddModelError(nameof(data.ProductName), "Tên mặt hàng không được để trống");

            if (data.CategoryID == 0)
                ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại hàng");

            if (data.SupplierID == 0)
                ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");

            if (string.IsNullOrWhiteSpace(data.Unit))
                ModelState.AddModelError(nameof(data.Unit), "Đơn vị tính không được để trống");

            if (data.Price <= 0)
                ModelState.AddModelError(nameof(data.Price), "Giá bán phải lớn hơn 0");
            
            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật mặt hàng";
                ViewBag.IsEdit = data.ProductID != 0;
                return View("Edit", data);
            }

            if (data.ProductID == 0)
            {
                int id = ProductDataService.AddProduct(data);
                if (id <= 0)
                {
                    ModelState.AddModelError(nameof(data.ProductName), "Có lỗi khi thêm mặt hàng");
                    ViewBag.Title = "Bổ sung mặt hàng";
                    ViewBag.IsEdit = false;
                    return View("Edit", data);
                }
            }
            else
            {
                bool result = ProductDataService.UpdateProduct(data);
                if (!result)
                {
                    ModelState.AddModelError(nameof(data.ProductName), "Có lỗi khi cập nhật mặt hàng");
                    ViewBag.Title = "Cập nhật mặt hàng";
                    ViewBag.IsEdit = true;
                    return View("Edit", data);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id = 0)
        {
            var model = ProductDataService.GetProduct(id);
            if (model == null)
                return RedirectToAction("Index");

            bool inUsed = ProductDataService.InUsedProduct(id);
            ViewBag.InUsed = inUsed;

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id = 0, string confirm = "")
        {
            // Kiểm tra lần cuối trước khi xóa
            if (ProductDataService.InUsedProduct(id))
            {
                var model = ProductDataService.GetProduct(id);
                ViewBag.InUsed = true;
                return View(model);
            }

            try
            {
                // Xóa ảnh và thuộc tính liên quan trước
                foreach (var photo in ProductDataService.ListPhotos(id))
                    ProductDataService.DeletePhoto(photo.PhotoID);

                foreach (var attr in ProductDataService.ListAttributes(id))
                    ProductDataService.DeleteAttribute(attr.AttributeID);

                // Xóa mặt hàng
                ProductDataService.DeleteProduct(id);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                var model = ProductDataService.GetProduct(id);
                ViewBag.InUsed = true;
                ViewBag.ErrorMessage = "Không thể xóa mặt hàng này vì có ràng buộc dữ liệu trong hệ thống.";
                return View(model);
            }
        }

        // ===================== PHOTO =====================

        [HttpGet]
        public IActionResult Photo(int id = 0, string method = "", int photoId = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh cho mặt hàng";
                    ViewBag.ProductID = id;
                    ViewBag.Method = "add";
                    var newPhoto = new ProductPhoto { ProductID = id, DisplayOrder = 1 };
                    return View("EditPhoto", newPhoto);

                case "edit":
                    ViewBag.Title = "Thay đổi ảnh của mặt hàng";
                    ViewBag.ProductID = id;
                    ViewBag.Method = "edit";
                    var photo = ProductDataService.GetPhoto(photoId);
                    if (photo == null)
                        return RedirectToAction("Edit", new { id });
                    return View("EditPhoto", photo);

                case "delete":
                    ProductDataService.DeletePhoto(photoId);
                    return RedirectToAction("Edit", new { id });

                default:
                    return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            // Xử lý upload file ảnh
            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                string savePath = Path.Combine(ApplicationContext.WWWRootPath, "images", "products", fileName);
                using (var stream = new FileStream(savePath, FileMode.Create))
                    uploadPhoto.CopyTo(stream);
                data.Photo = fileName;
            }
            else if (string.IsNullOrWhiteSpace(data.Photo))
            {
                data.Photo = "nophoto.png";
            }

            if (data.PhotoID == 0)
                ProductDataService.AddPhoto(data);
            else
                ProductDataService.UpdatePhoto(data);

            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        // ===================== ATTRIBUTE =====================

        [HttpGet]
        public IActionResult Attribute(int id = 0, string method = "", int attributeId = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính cho mặt hàng";
                    ViewBag.ProductID = id;
                    ViewBag.Method = "add";
                    var newAttr = new ProductAttribute { ProductID = id, DisplayOrder = 1 };
                    return View("EditAttribute", newAttr);

                case "edit":
                    ViewBag.Title = "Thay đổi thuộc tính của mặt hàng";
                    ViewBag.ProductID = id;
                    ViewBag.Method = "edit";
                    var attr = ProductDataService.GetAttribute(attributeId);
                    if (attr == null)
                        return RedirectToAction("Edit", new { id });
                    return View("EditAttribute", attr);

                case "delete":
                    ProductDataService.DeleteAttribute(attributeId);
                    return RedirectToAction("Edit", new { id });

                default:
                    return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult SaveAttribute(ProductAttribute data)
        {
            if (string.IsNullOrWhiteSpace(data.AttributeName))
                ModelState.AddModelError(nameof(data.AttributeName), "Tên thuộc tính không được để trống");
            if (string.IsNullOrWhiteSpace(data.AttributeValue))
                ModelState.AddModelError(nameof(data.AttributeValue), "Giá trị thuộc tính không được để trống");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.AttributeID == 0 ? "Bổ sung thuộc tính" : "Thay đổi thuộc tính";
                ViewBag.ProductID = data.ProductID;
                return View("EditAttribute", data);
            }

            if (data.AttributeID == 0)
                ProductDataService.AddAttribute(data);
            else
                ProductDataService.UpdateAttribute(data);

            return RedirectToAction("Edit", new { id = data.ProductID });
        }
    }
}
