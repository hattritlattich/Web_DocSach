using DocumentWebsite.Data;
using DocumentWebsite.Models;
using DocumentWebsite.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System.Security.Claims;

namespace DocumentWebsite.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDocumentRepo _documentRepo;
        private readonly IFavoriteRepo _favoriteRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoriteController(IFavoriteRepo favoriteRepo, UserManager<ApplicationUser> userManager, IDocumentRepo documentRepo, ApplicationDbContext context)
        {
            _context = context;
            _favoriteRepo = favoriteRepo;
            _userManager = userManager;
            _documentRepo = documentRepo;
        }

        public async Task<IActionResult> AddToFavorites(int documentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var documentExists = await _context.Documents.AnyAsync(d => d.Id == documentId);

            if (!documentExists)
            {
                throw new Exception("Tài liệu không tồn tại.");
            }

            if (!await _favoriteRepo.IsFavoriteAsync(userId, documentId))
            {
                await _favoriteRepo.AddToFavoritesAsync(userId, documentId);

                // Cập nhật số lượt thích
                var book = await _documentRepo.GetByIdAsync(documentId);
                if (book != null)
                {
                    book.FavoriteCount++;
                    await _documentRepo.UpdateAsync(book);
                }
            }

            return RedirectToAction("Details", "Document", new { id = documentId });
        }

        public async Task<IActionResult> RemoveFromFavorites(int documentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (await _favoriteRepo.IsFavoriteAsync(userId, documentId))
            {
                await _favoriteRepo.RemoveFromFavoritesAsync(userId, documentId);
                // Cập nhật số lượt thích
                var book = await _documentRepo.GetByIdAsync(documentId);
                if (book != null)
                {
                    book.FavoriteCount--;
                    await _documentRepo.UpdateAsync(book);
                }
            }

            return RedirectToAction("Details", "Document", new { id = documentId });
        }

        public async Task<IActionResult> RemoveFromMyFavorites(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (await _favoriteRepo.IsFavoriteAsync(userId, bookId))
            {
                await _favoriteRepo.RemoveFromFavoritesAsync(userId, bookId);
                // Cập nhật số lượt thích
                var book = await _documentRepo.GetByIdAsync(bookId);
                if (book != null)
                {
                    book.FavoriteCount--;
                    await _documentRepo.UpdateAsync(book);
                }
            }

            return RedirectToAction("MyFavorites", "Favorite");
        }

        public async Task<IActionResult> MyFavorites(int pageNumber = 1, int pageSize = 8)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Lấy danh sách yêu thích đã phân trang
            var favorites = await _favoriteRepo.GetFavoriteDocumentsAsync(user.Id, pageNumber, pageSize);

            // Lấy tổng số lượng yêu thích để tính tổng số trang
            var totalFavorites = await _favoriteRepo.GetFavoriteDocumentsCountAsync(user.Id);
            var totalPages = (int)Math.Ceiling(totalFavorites / (double)pageSize);

            // Lưu thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(favorites);
        }
    }
}
