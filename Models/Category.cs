using System.ComponentModel.DataAnnotations;

namespace DocumentWebsite.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string? Title { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public List<Document>? Documents { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
