namespace MeterApi.Models
{
    public class MeterReading
    {
        public int MeterReadingId { get; set; }
        public int AccountId { get; set; }
        public DateTime MeterReadingDate { get; set; }
        public int MeterReadingValue { get; set; }
    }
}
