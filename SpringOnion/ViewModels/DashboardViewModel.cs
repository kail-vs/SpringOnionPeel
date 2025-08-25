using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using SpringOnion.Data;
using SpringOnion.Data.Entities;
using System.Collections.ObjectModel;

namespace SpringOnion.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        [ObservableProperty]
        private ObservableCollection<UserProfile> users = new();

        [ObservableProperty]
        private string userCountText = "Users in DB: 0";

        public DashboardViewModel(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            LoadUsersAsync();
        }

        private async void LoadUsersAsync()
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var list = await db.UserProfiles
                .AsNoTracking()
                .OrderBy(u => u.DisplayName ?? u.UserId)
                .ToListAsync();

            Users = new ObservableCollection<UserProfile>(list);
            UserCountText = $"Users in DB: {list.Count}";
        }
    }
}
