using System.ComponentModel.DataAnnotations;

namespace DocumentWebsite.Models
{
    public class Document
    {
        public int Id { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }
        [StringLength(500)]
        public string? pdf { get; set; }

        [Required, StringLength(100)]
        public string? Title { get; set; }
        [StringLength(100)]
        public string? Author { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        //--------------------------------------------------------------------------------
        // Số lượt thích - Lượt xem
        public int FavoriteCount { get; set; }
        public int ViewCount { get; set; }
        public int DownloadCount { get; set; } = 0;

        // 27/11 - COMMENT
        public ICollection<Comment>? Comments { get; set; }
        public int CommentCount => Comments?.Count() ?? 0;

        // RATING        
        public double AverageRating { get; set; }
        public ICollection<Rating>? Ratings { get; set; } = new List<Rating>();
    }
}
