namespace FinanzasApp.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = "💰";
        public DateTime Date { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public enum TransactionType
    {
        Income,
        Expense
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = "📦";
        public TransactionType Type { get; set; }
        public string Color { get; set; } = "#6366F1";
    }

    public class Budget
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = "📦";
        public decimal LimitAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        // Propiedades normales con get/set — sin expression-body para evitar error TwoWay de WPF
        public decimal Percentage { get; set; }
        public string StatusColor { get; set; } = "#10B981";
        public double ProgressWidth { get; set; }
        public string PercentageText { get; set; } = "0";

        public void RecalcDerived()
        {
            Percentage = LimitAmount > 0 ? Math.Min((SpentAmount / LimitAmount) * 100, 100) : 0;
            StatusColor = Percentage >= 90 ? "#EF4444" : Percentage >= 70 ? "#F59E0B" : "#10B981";
            ProgressWidth = Math.Max(0, Math.Min((double)Percentage / 100.0 * 220.0, 220.0));
            PercentageText = ((int)Percentage).ToString();
        }
    }

    public class DashboardStats
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance { get; set; }  // normal property, no expression-body
        public List<CategoryStat> TopExpenseCategories { get; set; } = new();
        public List<MonthlyData> MonthlyData { get; set; } = new();
    }

    public class CategoryStat
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Icon { get; set; } = "📦";
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; } = "#6366F1";
    }

    public class MonthlyData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
    }
}
