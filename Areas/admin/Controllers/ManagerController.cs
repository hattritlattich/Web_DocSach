using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentWebsite.Data;
using Microsoft.AspNetCore.Authorization;

namespace DocumentWebsite.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "admin")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy số lượng người dùng
            var userCount = await _context.Users.CountAsync();

            // Lấy số lượng sách
            var bookCount = await _context.Documents.CountAsync();

            // Truyền dữ liệu qua ViewData hoặc ViewBag
            ViewData["UserCount"] = userCount;
            ViewData["BookCount"] = bookCount;

            // Truyền danh sách đơn hàng qua model
            return View();
        }
    }
}
