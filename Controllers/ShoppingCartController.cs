using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using WebBanSach.Extension;
using WebBanSach.Models;
using WebBanSach.Models.Entities;
using WebBanSach.Models.ViewModels;

namespace WebBanSach.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly BookstoreDbContext context;
        private readonly AiRecommendationClient _aiClient;
        public ShoppingCartController(BookstoreDbContext context, AiRecommendationClient aiClient)
        {
            this.context = context;
            _aiClient = aiClient;
        }


        //tao gio hàng rỗng
        public List<CartViewModel> GioHang
        {
            get
            {
                // Nếu đã đăng nhập, load thẳng từ CartItems
                if (User.Identity.IsAuthenticated)
                {
                    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var dbItems = context.CartItems
                        .Include(ci => ci.Book).ThenInclude(b => b.BookImages)
                        .Where(ci => ci.CustomerId == userId)
                        .ToList();

                    return dbItems.Select(ci => new CartViewModel
                    {
                        book = ci.Book,
                        amount = (int)ci.Quantity
                    }).ToList();
                }

                // Chưa login: fallback về session
                var gh = HttpContext.Session.Get<List<CartViewModel>>("GioHang");
                return gh ?? new List<CartViewModel>();
            }
        }

        [HttpPost]
        [Route("api/cart/add")]
        public IActionResult AddtoCart(int bookID, int? amount)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    // === Use DB ===
                    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var cartItem = context.CartItems
                        .SingleOrDefault(ci => ci.CustomerId == userId && ci.BookId == bookID);

                    if (cartItem != null)
                    {
                        cartItem.Quantity = amount ?? cartItem.Quantity + 1;
                    }
                    else
                    {
                        context.CartItems.Add(new CartItem
                        {
                            CustomerId = userId,
                            BookId = bookID,
                            Quantity = amount ?? 1
                        });
                    }
                    context.SaveChanges();

                    // Tính lại
                    var subtotal = context.CartItems
                        .Where(ci => ci.CustomerId == userId && ci.BookId == bookID)
                        .Select(ci => ci.Quantity * ci.Book.Price)
                        .Single();
                    var cartTotal = context.CartItems
                        .Where(ci => ci.CustomerId == userId)
                        .Sum(ci => ci.Quantity * ci.Book.Price);

                    return Json(new
                    {
                        success = true,
                        bookID = bookID,
                        quantity = amount ?? context.CartItems
                                      .Single(ci => ci.CustomerId == userId && ci.BookId == bookID).Quantity,
                        subtotal,
                        cartTotal
                    });
                }
                else
                {
                    // === Fallback session cũ ===
                    var gioHang = GioHang;
                    var item = gioHang.SingleOrDefault(p => p.book.BookId == bookID);
                    if (item != null)
                    {
                        item.amount = amount ?? (item.amount + 1);
                    }
                    else
                    {
                        var hh = context.Books.Single(p => p.BookId == bookID);
                        item = new CartViewModel { book = hh, amount = amount ?? 1 };
                        gioHang.Add(item);
                    }
                    HttpContext.Session.Set("GioHang", gioHang);

                    var subtotal = item.TotalMoney;
                    var cartTotal = gioHang.Sum(x => x.TotalMoney);

                    return Json(new
                    {
                        success = true,
                        bookID,
                        quantity = item.amount,
                        subtotal,
                        cartTotal
                    });
                }
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        [HttpPost]
        [Route("api/cart/remove")]
        public IActionResult Remove(int bookID)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var dbItem = context.CartItems
                                      .SingleOrDefault(ci => ci.CustomerId == userId && ci.BookId == bookID);
                    if (dbItem != null)
                    {
                        context.CartItems.Remove(dbItem);
                        context.SaveChanges();
                    }

                    // Tính lại tổng
                    var cartTotal = context.CartItems
                        .Where(ci => ci.CustomerId == userId)
                        .Sum(ci => ci.Quantity * ci.Book.Price);

                    return Json(new { success = true, cartTotal });
                }
                else
                {
                    var gioHang = GioHang;
                    var item = gioHang.SingleOrDefault(p => p.book.BookId == bookID);
                    if (item != null) gioHang.Remove(item);
                    HttpContext.Session.Set("GioHang", gioHang);

                    var cartTotal = gioHang.Sum(x => x.TotalMoney);
                    return Json(new { success = true, cartTotal });
                }
            }
            catch
            {
                return Json(new { success = false });
            }
        }
        [Route("cart.html", Name = "Cart")]
        public IActionResult Index()
        {
            var lsGioHang = GioHang;
            return View(GioHang);
        }

        [HttpGet]
        [Route("checkout.html", Name = "Checkout")]
        public IActionResult Checkout()
        {
            // 1. Load list phương thức thanh toán
            var paymentMethods = context.PaymentMethods.ToList();

            // 2. Chuẩn bị ViewModel
            var vm = new CheckoutViewModel
            {
                PaymentMethods = paymentMethods,
                CartItems = new List<CartViewModel>(),

                // Mặc định Quốc gia
                Country = "Việt Nam",
                // Mặc định chọn PM đầu tiên (nếu có)
                SelectedPaymentMethodId = paymentMethods.Any()
            ? paymentMethods.First().PaymentMethodId
            : 0
            };

            // 3. Nếu đã đăng nhập thì load từ DB, đồng thời tự fill thông tin nhận hàng
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var customer = context.Customers.Find(userId);

                // Prefill thông tin
                vm.RecipientName = customer.FullName;
                vm.Phone = customer.Phone;
                if (!string.IsNullOrEmpty(customer.Address))
                {
                    var parts = customer.Address.Split(',', StringSplitOptions.TrimEntries);

                    if (parts.Length >= 4)
                    {
                        vm.AddressDetail = parts[0];
                        vm.Ward = parts[1];
                        vm.District = parts[2];
                        vm.Province = parts[3];
                    }
                }

                // Lấy giỏ DB (bảng CartItems)
                vm.CartItems = context.CartItems
                    .Include(ci => ci.Book)
                    .Where(ci => ci.CustomerId == userId)
                    .Select(ci => new CartViewModel
                    {
                        book = ci.Book,
                        amount = (int)ci.Quantity
                    })
                    .ToList();
            }
            else
            {
                // 4. Chưa đăng nhập → load giỏ từ Session
                vm.CartItems = HttpContext.Session
                    .Get<List<CartViewModel>>("GioHang")
                    ?? new List<CartViewModel>();
            }
            // 5. Render view
            return View(vm);
        }
        [HttpPost("checkout.html")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            // Dò trạng thái tạo đơn khách vãng lai
            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            // 1. Load cartItems từ DB hoặc Session
            List<CartViewModel> cartItems;
            if (User.Identity.IsAuthenticated)
            {
                cartItems = context.CartItems
                    .Include(ci => ci.Book)
                    .Where(ci => ci.CustomerId == userId)
                    .Select(ci => new CartViewModel { book = ci.Book, amount = (int)ci.Quantity })
                    .ToList();
            }
            else
            {
                cartItems = HttpContext.Session.Get<List<CartViewModel>>("GioHang")
                            ?? new List<CartViewModel>();
            }

            // 2. Tạo Order
            var order = new Order
            {
                CustomerId = userId,
                CustomerName = model.RecipientName,
                OrderDate = DateTime.Now,
                ReceiveDate = DateTime.Now.AddDays(3),
                Address = $"{model.AddressDetail}, {model.Ward}, {model.District}, {model.Province}, {model.Country}",
                PaymentMethodId = model.SelectedPaymentMethodId,
                StatusId = 1, // chờ xác nhận
                TotalPrice = cartItems.Sum(ci => ci.book.Price * ci.amount)
            };
            context.Orders.Add(order);
            context.SaveChanges();

            // 3. Tạo OrderItems
            foreach (var item in cartItems)
            {
                context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    BookId = item.book.BookId,
                    BookPrice = item.book.Price,
                    BookQuantity = item.amount
                });
            }
            context.SaveChanges();
            var ok = await _aiClient.RetrainAsync();
            if (!ok)
            {
                TempData["ToastMessage"] = "Retrain fail!";
                TempData["ToastType"] = "error";
            }
            // 4. Xóa cart
            if (User.Identity.IsAuthenticated)
            {
                var old = context.CartItems.Where(ci => ci.CustomerId == userId);
                context.CartItems.RemoveRange(old);
                context.SaveChanges();
            }
            else
            {
                HttpContext.Session.Remove("GioHang");
            }

            TempData["ToastMessage"] = "Thanh toán thành công!";
            TempData["ToastType"] = "success"; // hoặc error, info...

            return RedirectToAction("Index", "Book");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reorder(int orderId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var order = context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == userId);

            if (order == null)
            {
                TempData["ToastMessage"] = "Không tìm thấy đơn hàng.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Profile", "Account");
            }

            foreach (var item in order.OrderItems)
            {
                var existing = context.CartItems
                    .FirstOrDefault(ci => ci.CustomerId == userId && ci.BookId == item.BookId);

                if (existing != null)
                {
                    existing.Quantity += item.BookQuantity ?? 1;
                }
                else
                {
                    context.CartItems.Add(new CartItem
                    {
                        CustomerId = userId,
                        BookId = item.BookId ?? 0,
                        Quantity = item.BookQuantity ?? 1
                    });
                }
            }

            context.SaveChanges();

            TempData["ToastMessage"] = "Đã thêm lại sản phẩm vào giỏ hàng.";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index");
        }

    }
}