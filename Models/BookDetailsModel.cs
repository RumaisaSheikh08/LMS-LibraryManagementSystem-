namespace LMSAvanza.Models
{
    public class BookDetailsModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Message { get; set; }

        public double BookId { get; set; }
    }
}
