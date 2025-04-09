using DocumentWebsite.Data;
using DocumentWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentWebsite.Repositories
{
    public class EFFavoriteRepo : IFavoriteRepo
    {
        private readonly ApplicationDbContext _context;

        public EFFavoriteRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsFavoriteAsync(string userId, int documentId)
        {
            return await _context.Favorites.AnyAsync(f => f.UserId == userId && f.DocumentId == documentId);
        }

        public async Task AddToFavoritesAsync(string userId, int documentId)
        {
            var favorite = new Favorite { UserId = userId, DocumentId = documentId };
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromFavoritesAsync(string userId, int documentId)
        {
            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.DocumentId == documentId);
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Document>> GetFavoriteDocumentsAsync(string userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.Document)
                .ToListAsync();
        }

        public async Task<int> GetFavoriteCountAsync(int documentId)
        {
            return await _context.Favorites.CountAsync(f => f.DocumentId == documentId);
        }

        // Phân trang
        public async Task<IEnumerable<Document>> GetFavoriteDocumentsAsync(string userId, int pageNumber, int pageSize)
        {
            return await _context.Favorites
                .Where(fb => fb.UserId == userId)
                .OrderBy(fb => fb.Document.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(fb => fb.Document)
                .Select(fb => fb.Document)
                .ToListAsync();
        }

        public async Task<int> GetFavoriteDocumentsCountAsync(string userId)
        {
            return await _context.Favorites
                .Where(fb => fb.UserId == userId)
                .CountAsync();
        }
    }
}
