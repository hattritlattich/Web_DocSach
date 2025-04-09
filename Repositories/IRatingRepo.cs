using DocumentWebsite.Models;

namespace DocumentWebsite.Repositories
{
    public interface IRatingRepo
    {
        Task AddRatingAsync(Rating rating);
        Task<IEnumerable<Rating>> GetRatingsByDocumentIdAsync(int documentId);
        Task<Rating> GetRatingByUserAndDocumentAsync(string userId, int documentId);

        // 27/11 - Quản lý đánh giá
        Task<IEnumerable<Rating>> GetAllRatingsAsync();
        Task<Rating> GetByIdAsync(int ratingId);
        Task DeleteRatingAsync(Rating rating);

        // LẤY ĐÁNH GIÁ TRUNG BÌNH
        Task<double?> GetAverageRatingAsync(int bookId);
        Task<Rating?> GetExistingRatingAsync(string userId, int bookId);
        Task UpdateRatingAsync(Rating rating);
    }
}
