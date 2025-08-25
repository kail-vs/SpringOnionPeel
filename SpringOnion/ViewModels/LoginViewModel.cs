using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.EntityFrameworkCore;
using SpringOnion.Data;
using SpringOnion.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SpringOnion.ViewModels
{
    public class LoginViewModel : BaseViewModel
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

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        public LoginViewModel(AuthenticationService authService, IDbContextFactory<AppDbContext> dbFactory, UserSyncService userSync)
        {
            _authService = authService;
            _dbFactory = dbFactory;
            _userSync = userSync;
            LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
            GoToRegisterCommand = new Command(async () => await GoToRegisterAsync());
        }

        private async Task LoginAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var (success, message) = await _authService.LoginAsync(UserId, Password);

                if (success)
                {
                    var toast = Toast.Make(message, ToastDuration.Short, 12);
                    await toast.Show();

                    await using var db = await _dbFactory.CreateDbContextAsync();
                    await db.Database.MigrateAsync();

                    var (ok, syncMsg) = await _userSync.SyncAllUsersAsync();

                    Application.Current.MainPage = new AppShellLayer();
                }
                else
                {
                    var toast = Toast.Make(message, ToastDuration.Short, 12);
                    await toast.Show();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GoToRegisterAsync()
        {
            await Shell.Current.GoToAsync("RegisterPage");
        }
    }
}
