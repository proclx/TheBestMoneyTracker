using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoneyRules.Domain.Enums;

namespace MoneyRules.Domain.Entities
{
    public class TransactionFilter
    {
        public int? UserId { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public TransactionType? Type { get; set; }
    }
}
