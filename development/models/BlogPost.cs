namespace Models
{
    public partial class BlogPost
    {
        public BlogPost()
        {
        }
        public int postId { get; set; }
        public string postImageUrl { get; set; }
        public string postTitle { get; set; }
        public string postBody { get; set; }
        public long createdAt { get; set; }
        public bool deleted { get; set; }
    }
}