using System.ComponentModel.DataAnnotations;

namespace DocumentWebsite.Models
{
    public class Rating
    {
        public int Id { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá phải trong khoảng từ 1 đến 5")]
        public int Star { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relationships
        public int DocumentId { get; set; }
        public Document Document { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
