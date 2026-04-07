using Microsoft.AspNetCore.Mvc;
using SV22T1020607.BusinessLayers;
using SV22T1020607.Shop.AppCodes;
using SV22T1020607.Shop.Models;
using System.Linq;

namespace SV22T1020607.Shop.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            var cart = HttpContext.GetCart();
            return View(cart);
        }

        public IActionResult Add(int id, int quantity = 1)
        {
            var cart = HttpContext.GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == id);
            
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                var product = ProductDataService.GetProduct(id);
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Photo = product.Photo,
                        SalePrice = product.Price,
                        Quantity = quantity
                    });
                }
            }

            HttpContext.SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = HttpContext.GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == id);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(int id, int quantity)
        {
            if (quantity <= 0) return Remove(id);

            var cart = HttpContext.GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == id);
            if (item != null)
            {
                item.Quantity = quantity;
                HttpContext.SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            HttpContext.ClearCart();
            return RedirectToAction("Index");
        }
    }
}
