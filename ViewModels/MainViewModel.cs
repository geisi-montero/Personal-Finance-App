using System.Collections.ObjectModel;
using FinanzasApp.Models;

namespace FinanzasApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentView;
        private string _currentPage = "Dashboard";
        private int _selectedNavIndex = 0;

        public BaseViewModel CurrentView { get => _currentView; set => Set(ref _currentView, value); }
        public string CurrentPage { get => _currentPage; set => Set(ref _currentPage, value); }
        public int SelectedNavIndex { get => _selectedNavIndex; set => Set(ref _selectedNavIndex, value); }

        public DashboardViewModel DashboardVM { get; }
        public TransactionsViewModel TransactionsVM { get; }
        public BudgetViewModel BudgetVM { get; }
        public CategoriesViewModel CategoriesVM { get; }

        public RelayCommand NavigateCommand { get; }

        public MainViewModel()
        {
            DashboardVM = new DashboardViewModel();
            TransactionsVM = new TransactionsViewModel();
            BudgetVM = new BudgetViewModel();
            CategoriesVM = new CategoriesViewModel();

            _currentView = DashboardVM;

            NavigateCommand = new RelayCommand(p =>
            {
                switch (p?.ToString())
                {
                    case "Dashboard":
                        CurrentView = DashboardVM;
                        CurrentPage = "Dashboard";
                        SelectedNavIndex = 0;
                        DashboardVM.Load();
                        break;
                    case "Transactions":
                        CurrentView = TransactionsVM;
                        CurrentPage = "Transacciones";
                        SelectedNavIndex = 1;
                        TransactionsVM.Load();
                        break;
                    case "Budget":
                        CurrentView = BudgetVM;
                        CurrentPage = "Presupuestos";
                        SelectedNavIndex = 2;
                        BudgetVM.Load();
                        break;
                    case "Categories":
                        CurrentView = CategoriesVM;
                        CurrentPage = "Categorías";
                        SelectedNavIndex = 3;
                        CategoriesVM.Load();
                        break;
                }
            });

            DashboardVM.Load();
        }
    }
}
