using DocumentWebsite.Models;

namespace DocumentWebsite.Repositories
{
    public interface IFavoriteRepo
    {
        Task<bool> IsFavoriteAsync(string userId, int documentId);
        Task AddToFavoritesAsync(string userId, int documentId);
        Task RemoveFromFavoritesAsync(string userId, int documentId);

        Task<IEnumerable<Document>> GetFavoriteDocumentsAsync(string userId);
        Task<int> GetFavoriteCountAsync(int documentId);

        // Phân trang
        Task<IEnumerable<Document>> GetFavoriteDocumentsAsync(string userId, int pageNumber, int pageSize);
        Task<int> GetFavoriteDocumentsCountAsync(string userId);
    }
}
