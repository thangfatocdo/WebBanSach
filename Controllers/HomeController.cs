using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebBanSach.Models.ViewModels;
using System.Security.Claims;
using WebBanSach.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebBanSach.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookstoreDbContext context;
        private readonly AiRecommendationClient _recService;



        public HomeController(ILogger<HomeController> logger, BookstoreDbContext context, AiRecommendationClient recService)
        {
            _logger = logger;
            this.context = context;
            _recService = recService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<Book> recBooks;
            

            if (int.TryParse(userIdStr, out var userId))
            {
                // Nếu đã login → gọi AI gợi ý
                var recIds = await _recService.RecommendAsync(userId);
                recBooks = context.Books.Include(b => b.BookImages).Where(b => recIds.Contains(b.BookId)).ToList();
            }
            else
            {
                // Nếu chưa login → fallback: sách có rating cao
                recBooks = context.BookRatings
                    .Where(r => r.RatingValue != null)
                    .GroupBy(r => r.BookId)
                    .Select(g => new
                    {
                        BookId = g.Key,
                        AvgRating = g.Average(r => r.RatingValue)
                    })
                    .OrderByDescending(x => x.AvgRating)
                    .Take(5)
                    .Join(context.Books, x => x.BookId, b => b.BookId, (x, b) => b)
                    .ToList();
            }
            var topBooks = context.Books.Include(b => b.BookImages).OrderByDescending(b => b.BookRatings.Average(r => r.RatingValue)).Take(6).ToList();ViewBag.TopRatedBooks = topBooks;     
            ViewBag.BookRatings = context.BookRatings.ToList();
            // Gán danh sách recommend hoặc fallback
            ViewBag.RecommendBooks = recBooks;

            // Lấy tất cả rating để hiện trong View
            ViewBag.BookRatings = context.BookRatings.ToList();

            return View();
        }




        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult About()
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
