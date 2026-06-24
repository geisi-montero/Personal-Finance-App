using System.Collections.ObjectModel;
using FinanzasApp.Data;
using FinanzasApp.Models;

namespace FinanzasApp.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly TransactionRepository _repo = new();
        private decimal _totalIncome, _totalExpenses, _balance;
        private string _selectedPeriod = "Este mes";
        private ObservableCollection<CategoryStat> _topCategories = new();
        private ObservableCollection<MonthlyBarItem> _monthlyBars = new();
        private ObservableCollection<Transaction> _recentTransactions = new();

        public decimal TotalIncome { get => _totalIncome; set => Set(ref _totalIncome, value); }
        public decimal TotalExpenses { get => _totalExpenses; set => Set(ref _totalExpenses, value); }
        public decimal Balance { get => _balance; set => Set(ref _balance, value); }
        public string SelectedPeriod { get => _selectedPeriod; set => Set(ref _selectedPeriod, value); }
        public ObservableCollection<CategoryStat> TopCategories { get => _topCategories; set => Set(ref _topCategories, value); }
        public ObservableCollection<MonthlyBarItem> MonthlyBars { get => _monthlyBars; set => Set(ref _monthlyBars, value); }
        public ObservableCollection<Transaction> RecentTransactions { get => _recentTransactions; set => Set(ref _recentTransactions, value); }
        public string CurrentMonthYear { get { return DateTime.Now.ToString("MMMM yyyy"); } }

        public void Load()
        {
            var stats = _repo.GetDashboardStats(DateTime.Now.Month, DateTime.Now.Year);
            TotalIncome = stats.TotalIncome;
            TotalExpenses = stats.TotalExpenses;
            Balance = stats.Balance;

            TopCategories = new ObservableCollection<CategoryStat>(stats.TopExpenseCategories);

            // Build bar chart data
            var maxVal = stats.MonthlyData.Max(m => Math.Max((double)m.Income, (double)m.Expenses));
            if (maxVal == 0) maxVal = 1;
            var bars = stats.MonthlyData.Select(m => new MonthlyBarItem
            {
                Month = m.Month,
                Income = m.Income,
                Expenses = m.Expenses,
                IncomeHeight = (double)(m.Income / (decimal)maxVal) * 120,
                ExpensesHeight = (double)(m.Expenses / (decimal)maxVal) * 120
            }).ToList();
            MonthlyBars = new ObservableCollection<MonthlyBarItem>(bars);

            // Recent transactions
            var recent = _repo.GetAll().Take(5).ToList();
            RecentTransactions = new ObservableCollection<Transaction>(recent);
        }
    }

    public class MonthlyBarItem
    {
        public string Month { get; set; } = "";
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public double IncomeHeight { get; set; }
        public double ExpensesHeight { get; set; }
    }
}
