using System.Collections.ObjectModel;
using FinanzasApp.Data;
using FinanzasApp.Models;

namespace FinanzasApp.ViewModels
{
    public class CategoriesViewModel : BaseViewModel
    {
        private readonly CategoryRepository _repo = new();
        private ObservableCollection<Category> _categories = new();
        private string _name = "";
        private string _icon = "📦";
        private string _selectedType = "Gasto";
        private string _selectedColor = "#6366F1";
        private bool _showForm = false;
        private string _formMessage = "";

        public ObservableCollection<Category> Categories { get => _categories; set => Set(ref _categories, value); }
        public string Name { get => _name; set => Set(ref _name, value); }
        public string Icon { get => _icon; set => Set(ref _icon, value); }
        public string SelectedType { get => _selectedType; set => Set(ref _selectedType, value); }
        public string SelectedColor { get => _selectedColor; set => Set(ref _selectedColor, value); }
        public bool ShowForm { get => _showForm; set => Set(ref _showForm, value); }
        public string FormMessage { get => _formMessage; set => Set(ref _formMessage, value); }

        public List<string> TypeOptions { get; } = new() { "Ingreso", "Gasto" };
        public List<string> ColorOptions { get; } = new()
        {
            "#6366F1","#3B82F6","#10B981","#F59E0B","#EF4444",
            "#EC4899","#8B5CF6","#14B8A6","#F97316","#A855F7"
        };

        public RelayCommand AddCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public CategoriesViewModel()
        {
            AddCommand = new RelayCommand(_ => { Name = ""; Icon = "📦"; SelectedType = "Gasto"; SelectedColor = "#6366F1"; FormMessage = ""; ShowForm = true; });
            CancelCommand = new RelayCommand(_ => ShowForm = false);
            SaveCommand = new RelayCommand(_ => Save());
            DeleteCommand = new RelayCommand(p => { if (p is int id) { _repo.Delete(id); Load(); } });
        }

        public void Load()
        {
            Categories = new ObservableCollection<Category>(_repo.GetAll());
        }

        private void Save()
        {
            FormMessage = "";
            if (string.IsNullOrWhiteSpace(Name)) { FormMessage = "El nombre es requerido."; return; }
            _repo.Add(new Category
            {
                Name = Name,
                Icon = string.IsNullOrWhiteSpace(Icon) ? "📦" : Icon,
                Type = SelectedType == "Ingreso" ? TransactionType.Income : TransactionType.Expense,
                Color = SelectedColor
            });
            ShowForm = false;
            Load();
        }
    }
}
