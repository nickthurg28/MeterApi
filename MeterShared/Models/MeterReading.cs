namespace MeterShared.Models
{
    public class MeterReading
    {
        public int MeterReadingId { get; set; }
        public int AccountId { get; set; }
        public DateTime MeterReadingDate { get; set; }
        public int MeterReadingValue { get; set; }

        public Account Account { get; set; }
    }
}
