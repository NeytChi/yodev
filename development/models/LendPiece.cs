namespace Models
{
    public partial class LendPiece
    {
        public LendPiece()
        {
        }
        public int lendId { get; set; }
        public string lendBody { get; set; }
        public long createdAt { get; set; }
        public bool deleted { get; set; }
    }
}