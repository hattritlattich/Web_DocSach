using DocumentWebsite.Models;

namespace DocumentWebsite.Repositories
{
    public interface ICommentRepo
    {
        Task AddCommentAsync(Comment comment);
        Task<IEnumerable<Comment>> GetCommentsByDocumentIdAsync(int documentId);
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment> GetByIdAsync(int id);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(int id);

        // 25/11 - Phản hồi comment
        Task AddReplyAsync(int parentCommentId, string userId, string content);
    }
}
