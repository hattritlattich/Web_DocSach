using DocumentWebsite.Models;

namespace DocumentWebsite.ViewModel
{
    public class HomeViewModel
    {
        public IEnumerable<Category>? Categories { get; set; }
        public IEnumerable<Document>? Documents { get; set; }

        // Top Favorited Books
        public IEnumerable<Document>? TopFavorites { get; set; }
        public int CurrentGroupFavorites { get; set; }
        public int TotalGroupsFavorites { get; set; }
        public int StartIndexFavorites { get; set; }


        // Top Viewed Books
        public IEnumerable<Document>? TopViewed { get; set; }
        public int CurrentGroupViewed { get; set; }
        public int TotalGroupsViewed { get; set; }
        public int StartIndexViewed { get; set; }
    }
}
