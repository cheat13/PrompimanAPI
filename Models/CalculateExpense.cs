using System.Collections.Generic;

namespace PrompimanAPI.Models
{
    public class CalculateExpense
    {
        public IEnumerable<Expense> ExpenseList { get; set; }
        public int TotalCost { get; set; }
        public int Paid { get; set; }
        public int Remaining { get; set; }
    }
}