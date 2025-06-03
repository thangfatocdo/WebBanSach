using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBanSach.Models.Entities;

namespace WebBanSach.Controllers
{
    public class BookController : Controller
    {
        private readonly BookstoreDbContext context;
        private readonly AiRecommendationClient _recService;
        public BookController(BookstoreDbContext context, AiRecommendationClient recService)
        {
            _recService = recService;
            this.context = context;
        }
        [Route("shop.html", Name = "BookShop")]
        public IActionResult Index(int? categoryId, int? page, decimal? minPrice, decimal? maxPrice, int? rating, string sort)
        {
            try
            {
                var pageNumber = page.GetValueOrDefault(1);
                const int pageSize = 8;

                // 1. Fetch all ratings (dùng cho hiển thị count)
                var allRatings = context.BookRatings.ToList();

                // 2. Build query động cho phép gắn thêm các điều kiện
                var query = context.Books.Include(b => b.BookImages).AsQueryable();

                if (categoryId.HasValue)
                    query = query.Where(b => b.CategoryId == categoryId.Value);

                if (minPrice.HasValue)
                    query = query.Where(b => b.Price >= minPrice.Value);
                if (maxPrice.HasValue)
                    query = query.Where(b => b.Price <= maxPrice.Value);
                // lọc sách có trung bình >= s và < s+1
                if (rating.HasValue)
                {
                    decimal s = rating.Value;
                    query = query.Where(b =>
                        context.BookRatings
                               .Where(r => r.BookId == b.BookId)
                               .Average(r => (decimal?)(r.RatingValue ?? 0)) >= s
                        && context.BookRatings
                               .Where(r => r.BookId == b.BookId)
                               .Average(r => (decimal?)(r.RatingValue ?? 0)) < s + 1
                    );
                }
                // CHỌN SORT
                switch (sort)
                {
                    case "price_desc":
                        query = query.Include(b => b.BookImages).OrderByDescending(b => b.Price);
                        break;
                    case "price_asc":
                        query = query.Include(b => b.BookImages).OrderBy(b => b.Price);
                        break;
                    case "latest":
                        query = query.Include(b => b.BookImages).OrderByDescending(b => b.CreatedAt);
                        break;
                    case "oldest":
                        query = query.Include(b => b.BookImages).OrderBy(b => b.CreatedAt);
                        break;
                    default:
                        query = query.Include(b => b.BookImages).OrderByDescending(b => b.BookId);
                        break;
                }
                // 3. Paging & other ViewBag
                var models = new PagedList<Book>(
                    query.AsNoTracking(),
                    pageNumber, pageSize);

                ViewBag.BookRatings = allRatings;
                ViewBag.SelectedRating = rating;
                ViewBag.Categories = context.Categories.OrderBy(c => c.CategoryName).ToList();
                ViewBag.SelectedCategoryId = categoryId;
                ViewBag.CurrentPage = pageNumber;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.SortOption = sort;

                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("/{id}.html", Name = "BookDetails")] //gán đường dẫn bên view index
        public async Task<IActionResult> BookDetail(int id)
        {
            try
            {
                var book = context.Books
                    .Include(b => b.Category)
                    .Include(b => b.Author)
                    .Include(b => b.Publisher)
                    .Include(b => b.BookImages) // 👈 dòng quan trọng
                    .FirstOrDefault(b => b.BookId == id);
                if (book == null)
                {
                    return RedirectToAction("Index");
                }

                // Lấy userId từ Claims (nếu chưa login -> không recommend)
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out var userId))
                {
                    // Trả về view với model rỗng
                    ViewBag.RecommendBooks = new List<Book>();
                }


                // Lấy danh sách sách gợi ý
                if (userIdStr != null)
                {
                    // Gọi AI gợi ý
                    var recIds = await _recService.RecommendAsync(userId, 10);
                    // Lấy sách tương ứng
                    var recBooks = context.Books.Include(b => b.BookImages).Where(b => recIds.Contains(b.BookId)).ToList();
                    ViewBag.RecommendBooks = recBooks;
                }

                // Lấy danh sách đánh giá cho sách
                var ratings = context.BookRatings
                    .Include(r => r.Customer)
                    .Where(r => r.BookId == id)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();
                ViewBag.BookRatings = ratings;
                // Nếu đã đăng nhập, kiểm tra user đã đánh giá chưa
                if (int.TryParse(userIdStr, out var currentUserId))
                {
                    var myRating = context.BookRatings.FirstOrDefault(r => r.BookId == id && r.CustomerId == currentUserId);
                    ViewBag.MyRating = myRating;
                }

                return View(book);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public IActionResult SubmitRating(BookRating model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login", "Account");

            var existing = context.BookRatings.FirstOrDefault(r => r.BookId == model.BookId && r.CustomerId == userId);
            if (existing != null)
            {
                // Cập nhật
                existing.RatingValue = model.RatingValue;
                existing.Comment = model.Comment;
                existing.CreatedAt = DateTime.Now;
            }
            else
            {
                // Thêm mới
                model.CustomerId = userId;
                model.CreatedAt = DateTime.Now;
                context.BookRatings.Add(model);
            }

            context.SaveChanges();
            return RedirectToRoute("BookDetails", new { id = model.BookId });
        }

        [HttpPost("api/upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { fileName });
        }

        [HttpGet]
        public IActionResult Search(string q, int? categoryId, int page = 1)
        {
            int pageSize = 8;

            var query = context.Books.Include(b => b.BookImages).AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(b => b.Title.Contains(q) || b.Author.AuthorName.Contains(q));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            var books = query
                .OrderByDescending(b => b.CreatedAt)
                .ToPagedList(page, pageSize);

            // Gán lại dữ liệu ViewBag để dùng chung với View Index
            ViewBag.CurrentPage = page;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Categories = context.Categories.ToList();
            ViewBag.BookRatings = context.BookRatings.ToList();
            ViewBag.SearchKeyword = q;

            return View("Index", books);
        }
    }
}