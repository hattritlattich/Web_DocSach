namespace DocumentWebsite.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int DocumentId  { get; set; }
        public Document Document { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
