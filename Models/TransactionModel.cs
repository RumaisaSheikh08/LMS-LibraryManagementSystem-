namespace LMSAvanza.Models
{
    public class TransactionModel
    {
        public string TxnID { get; set; }

        public string StudentID { get; set; }

        public double BookID { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public string Fee { get; set;}
         
        public string Reserved { get; set; }
        public string AmountStatus { get; set; }
    }
}
