namespace StreamingAPI.Models
{
    public class Content
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; } = string.Empty; // "movie" or "series"
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public int? Season { get; set; }      // only for series
        public int? Episodes { get; set; }    // only for series
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string VideoUrl { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;
    }
}