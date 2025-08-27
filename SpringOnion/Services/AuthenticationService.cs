using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System;

namespace SpringOnion.Services
{
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        private const string TokenKey = "auth_token";

        public string? Token { get; private set; }
        public string? UserId { get; private set; }
        public string? Email { get; private set; }

        public string BaseUrl => _baseUrl;

        private void ExtractClaimsFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token)) return;

            var jwt = handler.ReadJwtToken(token);

            UserId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                     ?? jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            Email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        }

        public AuthenticationService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _baseUrl = configuration["AuthApiBaseUrl"]
                       ?? throw new InvalidOperationException("AuthApiBaseUrl not configured.");
        }

        private static StringContent WriteJsonContent(object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string userId, string password)
        {
            var content = WriteJsonContent(new { userId, password });

            var response = await _httpClient.PostAsync($"{BaseUrl}/api/register", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await ReadJsonAsync<ApiResponse>(response);
                return (result?.Success ?? false, result?.Message ?? "Unknown error");
            }

            return (false, "Server error");
        }

        public async Task<(bool Success, string Message)> LoginAsync(string userId, string password)
        {
            var content = WriteJsonContent(new { userId, password });

            var response = await _httpClient.PostAsync($"{BaseUrl}/api/login", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await ReadJsonAsync<LoginResponse>(response);

                if (result != null && result.Success && !string.IsNullOrEmpty(result.Token))
                {
                    Token = result.Token;
                    await SecureStorage.SetAsync(TokenKey, Token);
                    ExtractClaimsFromToken(Token);
                    return (true, result.Message);
                }

                return (false, result?.Message ?? "Unknown error");
            }

            return (false, "Server error");
        }

        public async Task<bool> LoadTokenAsync()
        {
            Token = await SecureStorage.GetAsync(TokenKey);
            if (!string.IsNullOrEmpty(Token))
            {
                ExtractClaimsFromToken(Token);
                return true;
            }
            return false;
        }

        public async Task<(bool Success, string? Data)> GetProfileAsync()
        {
            if (string.IsNullOrEmpty(Token))
                return (false, "No token available");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Token);

            var response = await _httpClient.GetAsync($"{BaseUrl}/api/profile");

            if (response.IsSuccessStatusCode)
            {
                var result = await ReadJsonAsync<ApiResponseWithProfile>(response);
                if (result?.Success ?? false)
                {
                    UserId = result?.Data?.UserId;
                    Email = result?.Data?.Email;
                    return (true, UserId);
                }
                return (false, result?.Message ?? "Unknown error");
            }

            return (false, "Failed to fetch profile");
        }

        private class ApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        private class ApiResponseWithProfile : ApiResponse
        {
            public ProfileData? Data { get; set; }
        }

        private class ProfileData
        {
            public string UserId { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        private class LoginResponse : ApiResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}
