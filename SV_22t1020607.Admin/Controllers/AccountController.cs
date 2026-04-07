using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1020607.BusinessLayers;
using System.Security.Claims;
using LiteCommerce.Admin;

using Microsoft.AspNetCore.Authorization;

namespace SV22T1020607.Admin.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username = "", string password = "")
        {
            ViewBag.Username = username;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập tên và mật khẩu!");
                return View();
            }

            // Mã hoá mật khẩu bằng MD5 trước khi giao tiếp cơ sở dữ liệu
            string hashedPassword = LiteCommerce.Admin.CryptHelper.HashMD5(password);
            var userAccount = await UserAccountService.AuthorizeAsync(username, hashedPassword);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại!");
                return View();
            }

            // Khởi tạo Claims thông qua WebUserData chuẩn của dự án
            var userData = new LiteCommerce.Admin.WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = userAccount.RoleNames.Split(',').ToList()
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userData.CreatePrincipal(),
                new AuthenticationProperties());

            return RedirectToAction("Index", "Home");
        }
    
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword = "", string newPassword = "", string confirmPassword = "")
        {
            // Lấy thông tin user đang đăng nhập
            var userData = User.GetUserData();
            if (userData == null)
                return RedirectToAction("Login");

            // Validate đầu vào
            if (string.IsNullOrWhiteSpace(oldPassword))
                ModelState.AddModelError("oldPassword", "Vui lòng nhập mật khẩu cũ!");

            if (string.IsNullOrWhiteSpace(newPassword))
                ModelState.AddModelError("newPassword", "Vui lòng nhập mật khẩu mới!");
            else if (newPassword.Length < 6)
                ModelState.AddModelError("newPassword", "Mật khẩu mới phải có ít nhất 6 ký tự!");

            if (string.IsNullOrWhiteSpace(confirmPassword))
                ModelState.AddModelError("confirmPassword", "Vui lòng xác nhận mật khẩu mới!");
            else if (newPassword != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp!");

            if (!ModelState.IsValid)
                return View();

            // Kiểm tra mật khẩu cũ
            string hashedOld = LiteCommerce.Admin.CryptHelper.HashMD5(oldPassword);
            var check = await UserAccountService.AuthorizeAsync(userData.UserName, hashedOld);
            if (check == null)
            {
                ModelState.AddModelError("oldPassword", "Mật khẩu cũ không đúng!");
                return View();
            }

            // Đổi mật khẩu
            string hashedNew = LiteCommerce.Admin.CryptHelper.HashMD5(newPassword);
            bool result = await UserAccountService.ChangePasswordAsync(userData.UserName, hashedNew);
            if (!result)
            {
                ModelState.AddModelError("Error", "Đổi mật khẩu thất bại, vui lòng thử lại!");
                return View();
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ChangePassword");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        
        [AllowAnonymous]
        public IActionResult SeedData()
        {
            try 
            {
                var constr = SV22T1020607.BusinessLayers.Configuration.ConnectionString;
                var pass = LiteCommerce.Admin.CryptHelper.HashMD5("12345");
                var testAccounts = new System.Collections.Generic.List<(string Email, string FullName, string Role)>
                {
                    ("admin@litecommerce.com", "Admin System", "admin"),
                    ("sale@litecommerce.com", "Nhân viên Bán Hàng", "sales"),
                    ("data@litecommerce.com", "Nhân viên Dữ Liệu", "datamanager")
                };

                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(constr))
                {
                    conn.Open();
                    foreach (var acc in testAccounts)
                    {
                        var cmd = new Microsoft.Data.SqlClient.SqlCommand($"UPDATE Employees SET Password = '{pass}', RoleNames = '{acc.Role}' WHERE Email = '{acc.Email}'", conn);
                        int affected = cmd.ExecuteNonQuery();
                        if (affected == 0)
                        {
                            cmd.CommandText = $"INSERT INTO Employees(FullName, BirthDate, Email, Password, RoleNames, IsWorking) VALUES (N'{acc.FullName}', GETDATE(), '{acc.Email}', '{pass}', '{acc.Role}', 1)";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return Content("Đã tạo 3 User Test thành công!\n\n" +
                               "1. admin@litecommerce.com (Vai trò: admin) - MK: 12345\n" +
                               "2. sale@litecommerce.com (Vai trò: sales) - MK: 12345\n" +
                               "3. data@litecommerce.com (Vai trò: datamanager) - MK: 12345");
            }
            catch (Exception ex)
            {
                return Content("Lỗi khi tạo user: " + ex.Message);
            }
        }
    }
}
