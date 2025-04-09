
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DocumentWebsite.Data;
using DocumentWebsite.Models;
using DocumentWebsite.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace DocumentWebsite.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "admin")]
    public class DocumentManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDocumentRepo _bookRepo;
        private readonly ICommentRepo _commentRepo;
        private readonly IRatingRepo _ratingRepo;

        public DocumentManagerController(ApplicationDbContext context, IDocumentRepo bookRepo, ICommentRepo commentRepo, IRatingRepo ratingRepo)
        {
            _context = context;
            _bookRepo = bookRepo;
            _commentRepo = commentRepo;
            _ratingRepo = ratingRepo;
        }

        // GET: admin/BooksManager
        public async Task<IActionResult> Index(string sortColumn = "CreatedDate", string sortOrder = "desc", int pageNumber = 1, string query = "")
        {
            int pageSize = 10;

            // Khởi tạo truy vấn dữ liệu
            IQueryable<Document> documentsQuery = _context.Documents
                .Include(p => p.Category);

            // Áp dụng bộ lọc nếu có chuỗi tìm kiếm
            if (!string.IsNullOrEmpty(query))
            {
                documentsQuery = documentsQuery.Where(b => b.Title.Contains(query));
            }

            // Áp dụng sắp xếp theo cột và thứ tự
            switch (sortColumn.ToLower())
            {
                case "name":
                    documentsQuery = sortOrder == "asc" ? documentsQuery.OrderBy(b => b.Title) : documentsQuery.OrderByDescending(b => b.Title);
                    break;
                case "rating":
                    documentsQuery = sortOrder == "asc" ? documentsQuery.OrderBy(b => b.AverageRating) : documentsQuery.OrderByDescending(b => b.AverageRating);
                    break;
                case "comments":
                    documentsQuery = sortOrder == "asc" ? documentsQuery.OrderBy(b => b.Comments.Count) : documentsQuery.OrderByDescending(b => b.Comments.Count);
                    break;
                case "views":
                    documentsQuery = sortOrder == "asc" ? documentsQuery.OrderBy(b => b.ViewCount) : documentsQuery.OrderByDescending(b => b.ViewCount);
                    break;
                case "likes":
                    documentsQuery = sortOrder == "asc" ? documentsQuery.OrderBy(b => b.FavoriteCount) : documentsQuery.OrderByDescending(b => b.FavoriteCount);
                    break;                
                default:
                    // Mặc định sắp xếp theo CreatedDate giảm dần
                    documentsQuery = documentsQuery.OrderByDescending(b => b.CreatedDate);
                    break;
            }

            // Phân trang
            var paginated = await PaginatedList<Document>.CreateAsync(documentsQuery, pageNumber, pageSize);        

            // Truyền query vào ViewBag để giữ giá trị tìm kiếm trong View
            ViewBag.Query = query;
            // Truyền các tham số sắp xếp vào ViewBag để hiển thị trong view
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortOrder = sortOrder;

            return View(paginated);
        }


        // GET: admin/BooksManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Documents
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [Area("admin")]
        // GET: admin/BooksManager/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title");
            return View();
        }

        // POST: admin/BooksManager/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Area("admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> Create([Bind("Id,ImageUrl,Title,CategoryId,Author,Description,pdf,CreatedDate")] Document document, IFormFile imageUrl, IFormFile pdf)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    document.ImageUrl = await SaveImage(imageUrl);
                }

                if (pdf != null)
                {
                    document.pdf = await SavePDF(pdf);
                }

                _context.Add(document);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title", document.CategoryId);
            return View(document);
        }


        // GET: admin/BooksManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Documents.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title", book.CategoryId);
            return View(book);
        }

        // POST: admin/BooksManager/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,CategoryId,Author,Description")] Document document, IFormFile imageUrl, IFormFile pdf)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("pdf");

            if (id != document.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _bookRepo.GetByIdAsync(id);

                    if (imageUrl != null && imageUrl.Length > 0)
                        existingBook.ImageUrl = await SaveImage(imageUrl);

                    if (pdf != null && pdf.Length > 0)
                        existingBook.pdf = await SavePDF(pdf);

                    existingBook.Title = document.Title;
                    existingBook.CategoryId = document.CategoryId;
                    existingBook.Author = document.Author;
                    existingBook.Description = document.Description;

                    await _bookRepo.UpdateAsync(existingBook);
                    TempData["SuccessMessage"] = "Thông tin đã được cập nhật.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(document.Id)) return NotFound();
                    throw;
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title", document.CategoryId);
            return View(document);
        }

        // GET: admin/BooksManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Documents
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: admin/BooksManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Documents.FindAsync(id);
            if (book != null)
            {
                _context.Documents.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }

        // HÌNH ẢNH / TÀI LIỆU / ÂM THANH--------------------------------------------------
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            // Thay đổi đường dẫn theo cấu hình của bạn
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "" + image.FileName; // Trả về đường dẫn tương đối
        }

        private async Task<string> SavePDF(IFormFile pdf)
        {
            var savePath = Path.Combine("wwwroot/pdf", pdf.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await pdf.CopyToAsync(fileStream);
            }
            return "/pdf/" + pdf.FileName;
        }
        
        // XEM CHI TIẾT ĐÁNH GIÁ /BÌNH LUẬN -------------------------
        public IActionResult Ratings(int id)
        {
            var book = _context.Documents
                .Include(b => b.Ratings)
                .FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // Xóa đánh giá
        [HttpPost]
        public async Task<IActionResult> DeleteRating(int ratingId, int bookId)
        {
            // Lấy đánh giá cần xóa
            var rating = await _ratingRepo.GetByIdAsync(ratingId);
            if (rating == null)
            {
                return NotFound();
            }

            // Xóa đánh giá
            await _ratingRepo.DeleteRatingAsync(rating);

            // Cập nhật điểm đánh giá trung bình
            await _bookRepo.UpdateAverageRatingAsync(bookId);

            // Thông báo thành công
            ModelState.AddModelError(string.Empty, "Đánh giá đã được xóa!");

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // BÌNH LUẬN
        public IActionResult Comments(int id)
        {
            var book = _context.Documents
                .Include(b => b.Comments)
                    .ThenInclude(c => c.Replies) // Nếu có replies
                .FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // Xóa một bình luận
        public async Task<IActionResult> DeleteComment(int id, int bookId)
        {
            var comment = await _commentRepo.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            await _commentRepo.DeleteAsync(id);
            TempData["Success"] = "Comment has been deleted.";

            return Redirect(Request.Headers["Referer"].ToString());
        }        
    }
}
