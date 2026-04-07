using Microsoft.AspNetCore.Mvc;
using SV22T1020607.Shop.Models;
using System.Diagnostics;
using SV22T1020607.BusinessLayers;

namespace SV22T1020607.Shop.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var data = ProductDataService.ListProducts(1, 8, "", 0, 0, 0, 0);
            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
