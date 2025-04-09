using DocumentWebsite.Models;

namespace DocumentWebsite.Repositories
{
    public interface IDocumentRepo
    {
        Task<IEnumerable<Document>> GetAllAsync();
        Task<Document> GetByIdAsync(int id);
        Task AddAsync(Document Document);
        Task UpdateAsync(Document Document);
        Task DeleteAsync(int id);

        //Top Favorite - VIEW
        Task<IEnumerable<Document>> GetTopFavoritedDocumentsAsync(int count);
        Task<IEnumerable<Document>> GetTopViewedDocumentsAsync(int count);

        // RATING
        Task<double> GetAverageRatingByDocumentIdAsync(int DocumentId);
        Task UpdateAverageRatingAsync(int DocumentId);
    }
}
