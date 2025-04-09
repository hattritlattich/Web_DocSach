using System.ComponentModel.DataAnnotations;

namespace DocumentWebsite.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [StringLength(500)]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // 25/11 - Trường để lưu phản hồi
        public int? ParentId { get; set; }
        public Comment Parent { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

        // Relationships
        public int DocumentId { get; set; }
        public Document Document { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}