using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.EntityFrameworkCore;
using SpringOnion.Data;
using SpringOnion.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SpringOnion.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly UserSyncService _userSync;

        private string _userId;
        public string UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand RegisterCommand { get; }

        public RegisterViewModel(AuthenticationService authService, IDbContextFactory<AppDbContext> dbFactory, UserSyncService userSync)
        {
            _authService = authService;
            _dbFactory = dbFactory;
            _userSync = userSync;
            RegisterCommand = new Command(async () => await RegisterAsync());
        }

        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var (success, message) = await _authService.RegisterAsync(UserId, Password);

                if (success)
                {
                    if (success)
                    {
                        var toast = Toast.Make(message, ToastDuration.Short, 12);
                        toast.Show();

                        await using var db = await _dbFactory.CreateDbContextAsync();
                        await db.Database.MigrateAsync();

                        await Shell.Current.GoToAsync("LoginPage");
                    }
                }
                else
                {
                    var toast = Toast.Make(message, ToastDuration.Short, 12);
                    toast.Show();
                    //await App.Current.MainPage.DisplayAlert("Error", message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
