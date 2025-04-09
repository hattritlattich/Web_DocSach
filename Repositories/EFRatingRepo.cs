using BookWeb.Repositories;
using DocumentWebsite.Data;
using DocumentWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentWebsite.Repositories
{
    public class EFRatingRepo : IRatingRepo
    {
        private readonly ApplicationDbContext _context;

        public EFRatingRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRatingAsync(Rating rating)
        {
            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Rating>> GetRatingsByDocumentIdAsync(int documentId)
        {
            return await _context.Ratings
                .Where(r => r.DocumentId == documentId)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<Rating> GetRatingByUserAndDocumentAsync(string? userId, int documentId)
        {
            return await _context.Ratings.FirstOrDefaultAsync(r => r.UserId == userId && r.DocumentId == documentId);
        }

        // 27/11 - Quản lý đánh giá
        public async Task<IEnumerable<Rating>> GetAllRatingsAsync()
        {
            return await _context.Ratings.Include(r => r.DocumentId).ToListAsync();
        }

        public async Task<Rating> GetByIdAsync(int ratingId)
        {
            return await _context.Ratings.FirstOrDefaultAsync(r => r.Id == ratingId);
        }

        public async Task DeleteRatingAsync(Rating rating)
        {
            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();
        }       
        
        public async Task<double?> GetAverageRatingAsync(int documentId)
        {
            return await _context.Ratings
                .Where(r => r.DocumentId == documentId)
                .AverageAsync(r => (double?)r.Star);
        }

        public async Task<Rating?> GetExistingRatingAsync(string userId, int documentId)
        {
            return await _context.Ratings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.DocumentId == documentId);
        }
        public async Task UpdateRatingAsync(Rating rating)
        {
            _context.Ratings.Update(rating);
            await _context.SaveChangesAsync();
        }
    }
}
