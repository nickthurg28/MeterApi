using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterShared.Models
{
    public class MeterReadingDto
    {
        public int AccountId { get; set; }
        public DateTime MeterReadingDate { get; set; }
        public int MaterReadingValue { get; set; }
    }
}
