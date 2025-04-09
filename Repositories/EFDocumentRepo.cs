using DocumentWebsite.Data;
using DocumentWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentWebsite.Repositories
{
    public class EFDocumentRepo : IDocumentRepo
    {
        private readonly ApplicationDbContext _context;

        public EFDocumentRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Document>> GetAllAsync()
        {
            return await _context.Documents.Include(p => p.Category).ToListAsync();
        }

        public async Task<Document> GetByIdAsync(int id)
        {
            return await _context.Documents.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Document Document)
        {
            _context.Documents.Add(Document);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Document Document)
        {
            _context.Documents.Update(Document);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var Document = await _context.Documents.FindAsync(id);
            _context.Documents.Remove(Document);
            await _context.SaveChangesAsync();
        }

        //Top LIKE
        public async Task<IEnumerable<Document>> GetTopFavoritedDocumentsAsync(int count)
        {
            return await _context.Documents
                .OrderByDescending(b => b.FavoriteCount)
                .Take(count)
                .ToListAsync();
        }

        //Top VIEW
        public async Task<IEnumerable<Document>> GetTopViewedDocumentsAsync(int count)
        {
            return await _context.Documents
                .OrderByDescending(b => b.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        // RATING
        public async Task<double> GetAverageRatingByDocumentIdAsync(int DocumentId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.DocumentId == DocumentId)
                .ToListAsync();

            return ratings.Any() ? ratings.Average(r => r.Star) : 0;
        }

        public async Task UpdateAverageRatingAsync(int DocumentId)
        {            
            // Lấy tất cả đánh giá của sách
            var ratings = await _context.Ratings
                .Where(r => r.DocumentId == DocumentId)
                .ToListAsync();

            if (ratings.Any())
            {
                // Tính lại trung bình sao
                double averageRating = ratings.Average(r => r.Star);

                // Cập nhật giá trị trung bình vào bảng sách
                var Document = await _context.Documents.FirstOrDefaultAsync(b => b.Id == DocumentId);
                if (Document != null)
                {
                    Document.AverageRating = averageRating;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Nếu không có đánh giá, đặt trung bình là 0
                var Document = await _context.Documents.FirstOrDefaultAsync(b => b.Id == DocumentId);
                if (Document != null)
                {
                    Document.AverageRating = 0;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
