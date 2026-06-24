using System.Collections.ObjectModel;
using FinanzasApp.Data;
using FinanzasApp.Models;

namespace FinanzasApp.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        private readonly BudgetRepository _repo = new();
        private readonly CategoryRepository _catRepo = new();

        private ObservableCollection<Budget> _budgets = new();
        private ObservableCollection<Category> _expenseCategories = new();
        private Category? _selectedCategory;
        private string _limitAmount = "";
        private int _selectedMonth = DateTime.Now.Month;
        private int _selectedYear = DateTime.Now.Year;
        private bool _showForm = false;
        private string _formMessage = "";

        public ObservableCollection<Budget> Budgets { get => _budgets; set => Set(ref _budgets, value); }
        public ObservableCollection<Category> ExpenseCategories { get => _expenseCategories; set => Set(ref _expenseCategories, value); }
        public Category? SelectedCategory { get => _selectedCategory; set => Set(ref _selectedCategory, value); }
        public string LimitAmount { get => _limitAmount; set => Set(ref _limitAmount, value); }
        public int SelectedMonth { get => _selectedMonth; set { Set(ref _selectedMonth, value); Load(); } }
        public int SelectedYear { get => _selectedYear; set { Set(ref _selectedYear, value); Load(); } }
        public bool ShowForm { get => _showForm; set => Set(ref _showForm, value); }
        public string FormMessage { get => _formMessage; set => Set(ref _formMessage, value); }
        public string CurrentMonthLabel { get { return new DateTime(SelectedYear, SelectedMonth, 1).ToString("MMMM yyyy"); } }

        public List<string> Months { get; } = Enumerable.Range(1, 12).Select(m => new DateTime(2000, m, 1).ToString("MMMM")).ToList();
        public List<int> Years { get; } = Enumerable.Range(DateTime.Now.Year - 2, 5).ToList();

        public RelayCommand AddBudgetCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand PrevMonthCommand { get; }
        public RelayCommand NextMonthCommand { get; }

        public BudgetViewModel()
        {
            AddBudgetCommand = new RelayCommand(_ => { LimitAmount = ""; SelectedCategory = ExpenseCategories.FirstOrDefault(); FormMessage = ""; ShowForm = true; });
            CancelCommand = new RelayCommand(_ => ShowForm = false);
            SaveCommand = new RelayCommand(_ => Save());
            DeleteCommand = new RelayCommand(p => { if (p is int id) { _repo.Delete(id); Load(); } });
            PrevMonthCommand = new RelayCommand(_ =>
            {
                var d = new DateTime(SelectedYear, SelectedMonth, 1).AddMonths(-1);
                SelectedYear = d.Year; SelectedMonth = d.Month;
            });
            NextMonthCommand = new RelayCommand(_ =>
            {
                var d = new DateTime(SelectedYear, SelectedMonth, 1).AddMonths(1);
                SelectedYear = d.Year; SelectedMonth = d.Month;
            });
        }

        public void Load()
        {
            ExpenseCategories = new ObservableCollection<Category>(_catRepo.GetAll(TransactionType.Expense));
            SelectedCategory ??= ExpenseCategories.FirstOrDefault();
            Budgets = new ObservableCollection<Budget>(_repo.GetByMonth(SelectedMonth, SelectedYear));
            OnPropertyChanged(nameof(CurrentMonthLabel));
        }

        private void Save()
        {
            FormMessage = "";
            if (SelectedCategory == null) { FormMessage = "Selecciona una categoría."; return; }
            if (!decimal.TryParse(LimitAmount.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var limit) || limit <= 0)
            { FormMessage = "Monto inválido."; return; }
            _repo.Save(new Budget { CategoryId = SelectedCategory.Id, LimitAmount = limit, Month = SelectedMonth, Year = SelectedYear });
            ShowForm = false;
            Load();
        }
    }
}
