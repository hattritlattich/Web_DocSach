using DocumentWebsite.Data;
using DocumentWebsite.Models;
using DocumentWebsite.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentWebsite.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ForeignLanguageTopic(int pageNumber = 1)
        {
            int pageSize = 12;
            IQueryable<Document> productsQuery = _context.Documents
                .Include(p => p.Category)
                .Where(p => p.Title.Contains("Anh"));

            var paginated = await PaginatedList<Document>.CreateAsync(productsQuery, pageNumber, pageSize);
            return View(paginated);
        }

        public async Task<IActionResult> TechnologyTopic(int pageNumber = 1)
        {
            int pageSize = 12;
            // Danh sách từ khóa các môn khoa học
            var scienceSubjects = new[] { "Toán", "Vật lý", "Hóa", "Sinh", "Khoa học", "Công nghệ" };

            IQueryable<Document> productsQuery = _context.Documents
                .Include(p => p.Category)
                .Where(p => scienceSubjects.Any(subject => p.Title.Contains(subject)));

            var paginated = await PaginatedList<Document>.CreateAsync(productsQuery, pageNumber, pageSize);
            return View(paginated);
        }

        public async Task<IActionResult> MarketingTopic(int pageNumber = 1)
        {
            int pageSize = 12;
            // Danh sách từ khóa các môn khoa học
            var scienceSubjects = new[] { "Toán", "Giải tích", "Đại số", "Hình học" };

            IQueryable<Document> productsQuery = _context.Documents
                .Include(p => p.Category)
                .Where(p => scienceSubjects.Any(subject => p.Title.Contains(subject)));

            var paginated = await PaginatedList<Document>.CreateAsync(productsQuery, pageNumber, pageSize);
            return View(paginated);
        }

        public async Task<IActionResult> FoodTopic(int pageNumber = 1)
        {
            int pageSize = 12;
            // Danh sách từ khóa các môn xã hội
            var socialSubjects = new[] { "Văn", "Việt", "Sử", "Địa", "Triết", "Xã hội", "Tâm lý", "Kinh tế", "Giáo dục" };

            IQueryable<Document> productsQuery = _context.Documents
                .Include(p => p.Category)
                .Where(p => socialSubjects.Any(subject => p.Title.Contains(subject)));

            var paginated = await PaginatedList<Document>.CreateAsync(productsQuery, pageNumber, pageSize);
            return View(paginated);
        }
    }
}
