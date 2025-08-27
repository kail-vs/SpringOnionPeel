using Newtonsoft.Json;
using SpringOnion.Data.Entities;
using SpringOnion.Data.Repositories;
using System.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpringOnion.Services
{
    public class UserSyncService
    {
        private readonly AuthenticationService _auth;
        private readonly IUserRepository _users;
        private readonly HttpClient _http;

        public UserSyncService(AuthenticationService auth, IUserRepository users)
        {
            _auth = auth;
            _users = users;
            _http = new HttpClient();
        }

        public async Task<(bool Success, string Message)> SyncAllUsersAsync(CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_auth.Token))
                return (false, "Not authenticated");

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_auth.BaseUrl}/api/users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.Token);

            var resp = await _http.SendAsync(request, ct);
            if (!resp.IsSuccessStatusCode)
                return (false, $"Server error: {(int)resp.StatusCode}");

            var json = await resp.Content.ReadAsStringAsync(ct);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<UserDto>>>(json);

            if (result?.Success == true && result.Data != null)
            {
                var mapped = result.Data.Select(d => new UserProfile
                {
                    UserId = d.UserId,
                    DisplayName = d.DisplayName,
                    AvatarPath = null,
                    UpdatedAtUtc = d.UpdatedAtUtc.UtcDateTime,
                    CreatedAtUtc = d.UpdatedAtUtc.UtcDateTime
                });

                await _users.UpsertUsersAsync(mapped, ct);
                return (true, $"Synced {result.Data.Count} users");
            }

            return (false, result?.Message ?? "Unknown error");
        }

        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
        }

        private class UserDto
        {
            public string UserId { get; set; } = string.Empty;
            public string? DisplayName { get; set; }
            public DateTimeOffset UpdatedAtUtc { get; set; }
        }
    }
}
