using SpringOnion.ViewModels;

namespace SpringOnion.Views
{
    public partial class Dashboard : ContentPage
    {
        public Dashboard(DashboardViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
            _ = vm.InitAsync();
        }
    }
}
