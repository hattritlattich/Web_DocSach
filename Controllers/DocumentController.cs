using DocumentWebsite.Data;
using DocumentWebsite.Models;
using DocumentWebsite.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DocumentWebsite.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDocumentRepo _documentRepo;
        private readonly ICategoryRepo _categoryRepo;
        private readonly IFavoriteRepo _favoriteRepo;
        private readonly ICommentRepo _commentRepo;
        private readonly IRatingRepo _ratingRepo;

        public DocumentController(ApplicationDbContext context, IDocumentRepo documentRepo, ICategoryRepo categoryRepo, IFavoriteRepo favoriteRepo, ICommentRepo commentRepo, IRatingRepo ratingRepo)
        {
            _context = context;
            _documentRepo = documentRepo;
            _categoryRepo = categoryRepo;
            _favoriteRepo = favoriteRepo;
            _commentRepo = commentRepo;
            _ratingRepo = ratingRepo;
        }

        //----------------------------------------------
        // GET: documents
        public async Task<IActionResult> Index(string query, int? categoryId, string sortOrder, int pageNumber = 1)
        {
            int pageSize = 12;

            IQueryable<Document> documentsQuery = _context.Documents.Include(b => b.Category);

            // Lọc theo tên nếu có query
            if (!string.IsNullOrEmpty(query))
            {
                documentsQuery = documentsQuery.Where(b => b.Title.Contains(query));
            }

            // Lọc theo thể loại nếu có categoryId
            if (categoryId.HasValue)
            {
                documentsQuery = documentsQuery.Where(b => b.CategoryId == categoryId);
            }

            // Sắp xếp theo lựa chọn
            switch (sortOrder)
            {
                case "date_asc":
                    documentsQuery = documentsQuery.OrderBy(b => b.CreatedDate); // cũ nhất
                    break;
                case "date_desc":
                default:
                    documentsQuery = documentsQuery.OrderByDescending(b => b.CreatedDate); // mới nhất
                    break;
            }

            // Phân trang
            var paginatedDocuments = await PaginatedList<Document>.CreateAsync(documentsQuery, pageNumber, pageSize);

            // Lấy tất cả thể loại để hiển thị trong dropdown
            var categories = await _categoryRepo.GetAllAsync();
            ViewBag.Categories = categories.Select(x => new SelectListItem()
            {
                Text = x.Title,
                Value = x.Id.ToString(),
                Selected = categoryId.HasValue && categoryId.Value == x.Id
            });

            // Truyền categoryId vào ViewData để giữ giá trị trong dropdown
            ViewData["CategoryId"] = categoryId;
            ViewData["SortOrder"] = sortOrder;
            ViewData["Query"] = query;

            return PartialView(paginatedDocuments);
        }

        // PHÂN TRANG -------------------------------------------------------------
        public async Task<IActionResult> PagingNoLibrary(int pageNumber)
        {
            int pageSize = 5;
            IQueryable<Document> documentsQuery = _context.Documents.Include(p => p.Category);
            var pageds = await documentsQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return View(pageds);
        }

        // TÌM KIẾM -------------------------------------------------
        [HttpGet]
        public IActionResult SearchSuggestions(string query)
        {
            var suggestions = _context.Documents
                .Where(p => p.Title.StartsWith(query))
                .Select(p => new
                {
                    name = string.IsNullOrEmpty(p.Title) ? "Tên không xác định" : p.Title,
                    ImageUrl = string.IsNullOrEmpty(p.ImageUrl) ? "/images/default.jpg" : p.ImageUrl
                })
                .ToList();
            return Json(suggestions);
        }

        // 28/11 - FILTER / SORT-------------------------------------------------------------
        public async Task<IActionResult> SearchProducts(string query, int? categoryId, string sortOrder, int pageNumber = 1)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim(); // Loại bỏ khoảng trắng ở đầu và cuối chuỗi
            }

            IQueryable<Document> documentsQuery = _context.Documents.Include(b => b.Category);

            // Lọc theo tên nếu có query
            if (!string.IsNullOrEmpty(query))
            {
                documentsQuery = documentsQuery.Where(b => b.Title.Contains(query));
            }

            // Lọc theo thể loại nếu có categoryId
            if (categoryId.HasValue)
            {
                documentsQuery = documentsQuery.Where(b => b.CategoryId == categoryId);
            }

            // Sắp xếp theo lựa chọn
            switch (sortOrder)
            {
                case "date_asc":
                    documentsQuery = documentsQuery.OrderBy(b => b.CreatedDate); // cũ nhất
                    break;
                case "date_desc":
                default:
                    documentsQuery = documentsQuery.OrderByDescending(b => b.CreatedDate); // mới nhất
                    break;
            }

            // Phân trang
            var paginated = await PaginatedList<Document>.CreateAsync(documentsQuery, pageNumber, 12);

            // Lấy tất cả thể loại để hiển thị trong dropdown
            var categories = await _categoryRepo.GetAllAsync();
            ViewBag.Categories = categories.Select(x => new SelectListItem()
            {
                Text = x.Title,
                Value = x.Id.ToString(),
                Selected = categoryId.HasValue && categoryId.Value == x.Id
            });

            // Truyền categoryId vào ViewData để giữ giá trị trong dropdown
            ViewData["CategoryId"] = categoryId;
            ViewData["SortOrder"] = sortOrder;
            ViewData["Query"] = query;

            return PartialView(paginated);
        }

        //----------------------------------------------
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var document = await _context.Documents
                .Include(b => b.Category)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.Replies)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.User)
                .Include(b => b.Ratings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            // Tính toán số lượt đánh giá
            int ratingCount = document.Ratings.Count();
            // Lấy điểm đánh giá trung bình
            var averageRating = await _documentRepo.GetAverageRatingByDocumentIdAsync(id);

            // Tạo view model với thông tin sách và điểm trung bình
            var viewModel = new documentDetailsViewModel
            {
                Document = document,
                AverageRating = averageRating,
                // Sắp xếp bình luận theo ngày tạo
                Comments = document.Comments.OrderBy(c => c.CreatedAt).ToList(),
            };

            // Tăng số lượt xem
            document.ViewCount++;
            await _documentRepo.UpdateAsync(document);

            // Kiểm tra sách có nằm trong danh sách yêu thích của người dùng hay không
            var isFavorite = userId != null && await _favoriteRepo.IsFavoriteAsync(userId, id);
            // Kiểm tra người dùng đã đánh giá sách này chưa
            var existingRating = userId != null ? await _ratingRepo.GetRatingByUserAndDocumentAsync(userId, id) : null;

            // Lấy số sao đã đánh giá
            int ratedStar = existingRating?.Star ?? 0;
            // Nếu TempData chứa giá trị sao, ưu tiên sử dụng
            if (TempData.ContainsKey("RatedStar"))
            {
                ratedStar = Convert.ToInt32(TempData["RatedStar"]);
            }
            // Kiểm tra trạng thái đã đánh giá
            var hasRated = ratedStar > 0;

            // Truyền dữ liệu vào ViewBag và ViewData
            ViewData["IsFavorite"] = isFavorite;
            ViewData["HasRated"] = hasRated;
            ViewData["RatingCount"] = ratingCount;
            ViewBag.PdfPath = document.pdf;
            ViewBag.RatedStar = ratedStar;

            return View(document);
        }

        public class documentDetailsViewModel
        {
            public Document? Document { get; set; }
            public double AverageRating { get; set; }
            public List<Comment>? Comments { get; set; }
        }

        // DOWNLOAD -------------------------------------------------------------------
        [HttpGet("download/{documentId}")]
        public async Task<IActionResult> DownloadFile(int documentId)
        {
            // Tìm tài liệu theo ID
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
            {
                return NotFound("Tài liệu không tồn tại.");
            }

            // Tăng số lượt tải xuống
            document.DownloadCount++;
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();

            // Đường dẫn file (cập nhật để trỏ tới thư mục "pdf")
            var filePath = Path.Combine("wwwroot/pdf", document.pdf);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File không tồn tại.");
            }

            // Trả file PDF về người dùng
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", document.pdf);
        }

        // RATING -------------------------------------------------------------------
        // Thêm đánh giá
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddRating(int documentId, int star)
        {
            if (star < 1 || star > 5)
            {
                return BadRequest("Rating must be between 1 and 5 stars.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem tài liệu có tồn tại hay không
            var document = await _documentRepo.GetByIdAsync(documentId);
            if (document == null)
            {
                return NotFound("Tài liệu không tồn tại.");
            }

            // Kiểm tra xem người dùng đã đánh giá sách này chưa
            var existingRating = await _ratingRepo.GetExistingRatingAsync(userId, documentId);
            if (existingRating != null)
            {
                // Nếu đã đánh giá, cập nhật sao
                existingRating.Star = star;
                existingRating.CreatedAt = DateTime.UtcNow;
                await _ratingRepo.UpdateRatingAsync(existingRating);
            }
            else
            {
                // Nếu chưa đánh giá, thêm mới
                var rating = new Rating
                {
                    DocumentId = documentId,
                    UserId = userId,
                    Star = star,
                    CreatedAt = DateTime.UtcNow
                };

                await _ratingRepo.AddRatingAsync(rating);
            }

            // Cập nhật điểm đánh giá trung bình
            await _documentRepo.UpdateAverageRatingAsync(documentId);

            // Cập nhật ViewBag.RatedStar cho thông báo
            TempData["RatedStar"] = star;

            // Chuyển hướng về trang chi tiết
            return RedirectToAction("Details", "Document", new { id = documentId });
        }
    }
}
