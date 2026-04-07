using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020607.BusinessLayers;
using SV22T1020607.Shop.AppCodes;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV22T1020607.Shop.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username = "", string password = "", string returnUrl = "")
        {
            ViewBag.Username = username;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập thông tin đầy đủ!");
                return View();
            }

            // Gọi Customer Auth
            var userAccount = await UserAccountService.AuthorizeCustomerAsync(username, password);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Tài khoản hoặc mật khẩu không chính xác!");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userAccount.UserId),
                new Claim(ClaimTypes.Name, userAccount.DisplayName),
                new Claim(ClaimTypes.Email, userAccount.UserName),
                new Claim(ClaimTypes.Role, userAccount.RoleNames)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = false };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Gộp giỏ hàng của khách ẩn danh vào giỏ hàng của user
            HttpContext.MergeCartAfterLogin();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            ViewBag.Provinces = await SV22T1020607.BusinessLayers.DictionaryDataService.ListProvincesAsync();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string customerName, string contactName, string email, string phone, string address, string province, string password, string confirmPassword)
        {
            ViewBag.Provinces = await SV22T1020607.BusinessLayers.DictionaryDataService.ListProvincesAsync();
            if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(province))
            {
                ModelState.AddModelError("Error", "Vui lòng điền đủ Tên, Email, Mật khẩu và Tỉnh/Thành!");
                return View();
            }
            if (password != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp!");
                return View();
            }

            bool ok = await UserAccountService.RegisterCustomerAsync(customerName, contactName, email, phone, address, province, password);
            if (!ok)
            {
                ModelState.AddModelError("Error", "Email này đã được sử dụng!");
                return View();
            }

            TempData["SuccessMessage"] = "Đăng ký thành công! Hãy đăng nhập.";
            return RedirectToAction("Login");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Giữ nguyên Session (không Clear) để khách hàng vẫn có thể giữ giỏ hàng như Shopee
            return RedirectToAction("Login");
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            string userName = User.FindFirstValue(ClaimTypes.Email) ?? "";
            
            // Check old password
            var user = await UserAccountService.AuthorizeCustomerAsync(userName, oldPassword);
            if (user == null)
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ không chính xác.");
                return View();
            }

            bool ok = await UserAccountService.ChangeCustomerPasswordAsync(userName, newPassword);
            if (ok) {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Profile", "Account");
            }
            
            ModelState.AddModelError("Error", "Đổi mật khẩu thất bại.");
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out int customerId)) return RedirectToAction("Login");

            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            if (customer == null) return RedirectToAction("Login");

            return View(customer);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(string DisplayName, string Phone, string Address)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out int customerId)) return RedirectToAction("Login");

            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            if (customer == null) return RedirectToAction("Login");

            customer.CustomerName = DisplayName ?? "";
            customer.ContactName = DisplayName ?? ""; // Giữ đồng nhất tên
            customer.Phone = Phone ?? "";
            customer.Address = Address ?? "";

            bool success = await PartnerDataService.UpdateCustomerAsync(customer);
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }
            else
            {
                // Có thể dùng TempData để hiển thị lỗi
                ModelState.AddModelError("Error", "Cập nhật thông tin thất bại.");
            }

            return RedirectToAction("Profile");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
