namespace webKani.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public double Price { get; set; }
        public int BorrowCount { get; set; } // Tracks how many times the book was borrowed
    }
}
