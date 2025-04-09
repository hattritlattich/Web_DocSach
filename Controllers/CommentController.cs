using DocumentWebsite.Models;
using DocumentWebsite.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocumentWebsite.Controllers
{
    public class CommentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICommentRepo _commentRepo;

        public CommentController(UserManager<ApplicationUser> userManager, ICommentRepo commentRepo)
        {
            _userManager = userManager;
            _commentRepo = commentRepo;
        }

        [HttpPost("Add")]
        [Authorize]
        public async Task<IActionResult> AddComment(int documentId, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Bình luận không được để trống.";
                return RedirectToAction("Details", "Document", new { id = documentId });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không hợp lệ.";
                return RedirectToAction("Details", "Document", new { id = documentId });
            }

            var comment = new Comment
            {
                DocumentId = documentId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            comment.User = await _userManager.FindByIdAsync(userId);
            await _commentRepo.AddCommentAsync(comment);

            return RedirectToAction("Details", "Document", new { id = documentId });
        }

        // Xóa một bình luận
        public async Task<IActionResult> DeleteComment(int id, int documentId)
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

        // Phản hồi comment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReply(int parentId, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kiểm tra nội dung phản hồi không rỗng
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Phản hồi không được để trống.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Lấy thông tin bình luận cha
            var parentComment = await _commentRepo.GetByIdAsync(parentId);
            if (parentComment == null)
            {
                TempData["Error"] = "Bình luận không tồn tại.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Tạo đối tượng phản hồi mới
            var reply = new Comment
            {
                DocumentId = parentComment.DocumentId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ParentId = parentId
            };

            // Thêm phản hồi vào cơ sở dữ liệu
            await _commentRepo.AddCommentAsync(reply);
            TempData["Success"] = "Phản hồi đã được thêm.";

            // Truyền lại thông tin bình luận và phản hồi mới
            var comments = await _commentRepo.GetCommentsByDocumentIdAsync(parentComment.DocumentId);

            return RedirectToAction("Details", "Document", new { id = parentComment.DocumentId});
        }
    }
}
