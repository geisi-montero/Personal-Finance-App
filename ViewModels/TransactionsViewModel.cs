using System.Collections.ObjectModel;
using FinanzasApp.Data;
using FinanzasApp.Models;

namespace FinanzasApp.ViewModels
{
    public class TransactionsViewModel : BaseViewModel
    {
        private readonly TransactionRepository _repo = new();
        private readonly CategoryRepository _catRepo = new();

        private ObservableCollection<Transaction> _transactions = new();
        private ObservableCollection<Category> _categories = new();
        private Category? _selectedFilterCategory;
        private string _selectedFilterType = "Todos";
        private DateTime _filterFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        private DateTime _filterTo = DateTime.Now;

        // Form fields
        private string _title = "";
        private string _amount = "";
        private string _transactionType = "Gasto";
        private Category? _selectedCategory;
        private DateTime _date = DateTime.Today;
        private string _notes = "";
        private bool _showForm = false;
        private string _formMessage = "";

        public ObservableCollection<Transaction> Transactions { get => _transactions; set => Set(ref _transactions, value); }
        public ObservableCollection<Category> Categories { get => _categories; set => Set(ref _categories, value); }
        public ObservableCollection<Category> FilterCategories { get; private set; } = new();
        public List<string> TypeOptions { get; } = new() { "Todos", "Ingresos", "Gastos" };
        public List<string> TransactionTypeOptions { get; } = new() { "Ingreso", "Gasto" };

        public Category? SelectedFilterCategory { get => _selectedFilterCategory; set { Set(ref _selectedFilterCategory, value); ApplyFilters(); } }
        public string SelectedFilterType { get => _selectedFilterType; set { Set(ref _selectedFilterType, value); UpdateCategories(); ApplyFilters(); } }
        public DateTime FilterFrom { get => _filterFrom; set { Set(ref _filterFrom, value); ApplyFilters(); } }
        public DateTime FilterTo { get => _filterTo; set { Set(ref _filterTo, value); ApplyFilters(); } }

        public string Title { get => _title; set => Set(ref _title, value); }
        public string Amount { get => _amount; set => Set(ref _amount, value); }
        public string TransactionType { get => _transactionType; set { Set(ref _transactionType, value); UpdateFormCategories(); } }
        public Category? SelectedCategory { get => _selectedCategory; set => Set(ref _selectedCategory, value); }
        public DateTime Date { get => _date; set => Set(ref _date, value); }
        public string Notes { get => _notes; set => Set(ref _notes, value); }
        public bool ShowForm { get => _showForm; set => Set(ref _showForm, value); }
        public string FormMessage { get => _formMessage; set => Set(ref _formMessage, value); }

        public RelayCommand AddTransactionCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ClearFiltersCommand { get; }
        public RelayCommand SetTypeIncomeCommand { get; }
        public RelayCommand SetTypeExpenseCommand { get; }

        public TransactionsViewModel()
        {
            AddTransactionCommand = new RelayCommand(_ => { ClearForm(); ShowForm = true; });
            CancelCommand = new RelayCommand(_ => { ShowForm = false; FormMessage = ""; });
            SetTypeIncomeCommand = new RelayCommand(_ => { TransactionType = "Ingreso"; });
            SetTypeExpenseCommand = new RelayCommand(_ => { TransactionType = "Gasto"; });
            SaveCommand = new RelayCommand(_ => Save());
            DeleteCommand = new RelayCommand(p => { if (p is int id) { _repo.Delete(id); Load(); } });
            ClearFiltersCommand = new RelayCommand(_ =>
            {
                SelectedFilterType = "Todos";
                SelectedFilterCategory = null;
                FilterFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                FilterTo = DateTime.Now;
            });
        }

        public void Load()
        {
            var allCats = _catRepo.GetAll();
            Categories = new ObservableCollection<Category>(allCats);
            FilterCategories = new ObservableCollection<Category>(allCats);
            OnPropertyChanged(nameof(FilterCategories));
            UpdateFormCategories();
            ApplyFilters();
        }

        private void UpdateCategories()
        {
            var allCats = _catRepo.GetAll();
            if (_selectedFilterType == "Ingresos") FilterCategories = new ObservableCollection<Category>(allCats.Where(c => c.Type == Models.TransactionType.Income));
            else if (_selectedFilterType == "Gastos") FilterCategories = new ObservableCollection<Category>(allCats.Where(c => c.Type == Models.TransactionType.Expense));
            else FilterCategories = new ObservableCollection<Category>(allCats);
            OnPropertyChanged(nameof(FilterCategories));
            SelectedFilterCategory = null;
        }

        private void UpdateFormCategories()
        {
            var t = TransactionType == "Ingreso" ? Models.TransactionType.Income : Models.TransactionType.Expense;
            var cats = _catRepo.GetAll(t);
            Categories = new ObservableCollection<Category>(cats);
            SelectedCategory = Categories.FirstOrDefault();
        }

        private void ApplyFilters()
        {
            Models.TransactionType? typeFilter = SelectedFilterType switch
            {
                "Ingresos" => Models.TransactionType.Income,
                "Gastos" => Models.TransactionType.Expense,
                _ => null
            };
            var results = _repo.GetAll(FilterFrom, FilterTo, SelectedFilterCategory?.Id, typeFilter);
            Transactions = new ObservableCollection<Transaction>(results);
        }

        private void ClearForm()
        {
            Title = ""; Amount = ""; TransactionType = "Gasto"; Date = DateTime.Today; Notes = ""; FormMessage = "";
            UpdateFormCategories();
        }

        private void Save()
        {
            FormMessage = "";
            if (string.IsNullOrWhiteSpace(Title)) { FormMessage = "El título es requerido."; return; }
            if (!decimal.TryParse(Amount.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amt) || amt <= 0)
            { FormMessage = "Monto inválido."; return; }
            if (SelectedCategory == null) { FormMessage = "Selecciona una categoría."; return; }

            _repo.Add(new Transaction
            {
                Title = Title,
                Amount = amt,
                Type = TransactionType == "Ingreso" ? Models.TransactionType.Income : Models.TransactionType.Expense,
                CategoryId = SelectedCategory.Id,
                Date = Date,
                Notes = Notes
            });
            ShowForm = false;
            Load();
        }
    }
}
