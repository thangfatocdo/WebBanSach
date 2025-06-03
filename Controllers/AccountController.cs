using Microsoft.AspNetCore.Mvc;
using WebBanSach.Models.ViewModels;
using WebBanSach.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WebBanSach.Extension;
using WebBanSach.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace WebBanSach.Controllers
{
    public class AccountController : Controller
    {
        private readonly BookstoreDbContext context;
        private readonly PasswordHasher<Customer> _passwordHasher = new();
        public AccountController(BookstoreDbContext context)
        {
            this.context = context;
        }

        public IActionResult Register()
        {
            return View();
        }
        //Đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModels model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem email đã tồn tại chưa
                var existingCustomer = context.Customers.FirstOrDefault(c => c.Email == model.Email);
                if (existingCustomer != null)
                {
                    ModelState.AddModelError("", "Email đã được đăng ký.");
                    return View(model);
                }

                // Add thong tin vào entity
                var customer = new Customer
                {
                    FullName = model.Name,
                    Email = model.Email,
                    // TODO: nên hash nếu triển khai thật
                };
                // Hash password
                customer.Password = _passwordHasher.HashPassword(customer, model.Password);
                context.Customers.Add(customer);
                context.SaveChanges();

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }
        //Đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = context.Customers
                    .FirstOrDefault(c => c.Email == model.Email);

                if (customer != null)
                {
                    var result = _passwordHasher.VerifyHashedPassword(customer, customer.Password, model.Password);
                    if (result == PasswordVerificationResult.Success)
                    {
                        // Tạo Claims
                        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                new Claim(ClaimTypes.Name, customer.FullName),
                new Claim(ClaimTypes.Email, customer.Email)
            };
                        // 1. Xóa session cart cũ
                        HttpContext.Session.Remove("GioHang");

                        // 2. Lấy cart từ DB (CartItems) cho customer này
                        var dbCart = context.CartItems
                            .Include(ci => ci.Book)
                            .Where(ci => ci.CustomerId == customer.CustomerId)
                            .ToList();

                        // 3. Chuyển thành ViewModel
                        var vmCart = dbCart.Select(ci => new CartViewModel
                        {
                            book = ci.Book,
                            amount = (int)ci.Quantity
                        }).ToList();

                        var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                        var principal = new ClaimsPrincipal(identity);

                        // Ghi cookie
                        await HttpContext.SignInAsync("MyCookieAuth", principal, new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe // từ checkbox
                        });

                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }

        //Xác thực email
        public IActionResult VerifyEmail()
        {
            return View();
        }
        //Xác thực email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await context.Customers.FirstOrDefaultAsync(c => c.Email == model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Something is wrong!");
                    return View(model);
                }
                else
                {
                    return RedirectToAction("ChangePassword", "Account", new { username = user.Email });
                }
            }
            return View(model);
        }

        //Đổi mật khẩu
        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }
            return View(new ChangePasswordViewModel { Email = username });
        }

        //Đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm khách hàng theo email
                var customer = await context.Customers.FirstOrDefaultAsync(c => c.Email == model.Email);

                if (customer != null)
                {
                    // Hash mật khẩu mới
                    var hashedPassword = _passwordHasher.HashPassword(customer, model.NewPassword);

                    // Gán lại mật khẩu mới đã mã hóa
                    customer.Password = hashedPassword;

                    // Lưu vào DB
                    await context.SaveChangesAsync();

                    // Chuyển về trang Login
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy tài khoản với email này.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Vui lòng kiểm tra lại thông tin.");
            }

            return View(model);
        }
        //đăng xuất
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index", "Home");
        }
        //Trang Profile
        public IActionResult Profile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return RedirectToAction("Login");

            var customer = context.Customers.Find(userId);
            ViewBag.Customer = customer;

            var orders = context.Orders
                .Where(o => o.CustomerId == userId)
                .Include(o => o.Status)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.MyOrders = orders;
            return View();
        }
        //Update địa chỉ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAddress(string country, string province, string district, string ward, string detail)
        {
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var customer = context.Customers.FirstOrDefault(c => c.CustomerId == customerId);

            if (customer != null)
            {
                customer.Address = $"{detail}, {ward}, {district}, {province}, {country}";
                context.SaveChanges();
            }
            TempData["ToastMessage"] = "Lưu địa chỉ thành công!";
            TempData["ToastType"] = "success"; 
            return RedirectToAction("Profile");
        }

        [HttpGet("/Order/Details/{id}")]
        public IActionResult Details(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var order = context.Orders
                .Include(o => o.Status)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .FirstOrDefault(o => o.OrderId == id && o.CustomerId == userId);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var order = context.Orders.FirstOrDefault(o => o.OrderId == id && o.CustomerId == userId);

            if (order == null || order.StatusId != 1) // Chỉ cho huỷ khi trạng thái là "Chờ xác nhận"
                return BadRequest();

            order.StatusId = 5; // ví dụ 5 là trạng thái "Đã huỷ"
            context.SaveChanges();

            TempData["ToastMessage"] = "Huỷ đơn hàng thành công!";
            TempData["ToastType"] = "success";

            return RedirectToAction("Profile", "Account");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(string Email, string FullName, string Phone)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId))
            {
                var customer = context.Customers.Find(userId);
                if (customer != null)
                {
                    customer.Email = Email;
                    customer.FullName = FullName;
                    customer.Phone = Phone;

                    context.SaveChanges();
                    TempData["ToastMessage"] = "Lưu thông tin thành công!";
                    TempData["ToastType"] = "success";
                }
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Profile");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var customer = await context.Customers.FindAsync(userId);
            if (customer == null)
            {
                TempData["ToastMessage"] = "Người dùng không tồn tại.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Profile");
            }

            // 1. Verify mật khẩu cũ
            var result = _passwordHasher.VerifyHashedPassword(customer, customer.Password, model.OldPassword);
            if (result != PasswordVerificationResult.Success)
            {
                TempData["ToastMessage"] = "Mật khẩu cũ không đúng.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Profile");
            }

            // 2. Hash mật khẩu mới và lưu
            customer.Password = _passwordHasher.HashPassword(customer, model.NewPassword);
            await context.SaveChangesAsync();

            TempData["ToastMessage"] = "Đổi mật khẩu thành công!";
            TempData["ToastType"] = "success";
            return RedirectToAction("Profile");
        }



    }
}