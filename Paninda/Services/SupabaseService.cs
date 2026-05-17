using System.Net.Http.Json;
using System.Text.Json;
using Paninda.Models;

namespace Paninda.Services;

public class SupabaseService
{
    private const string Url = "https://pbeihlxbxhxgehmtyjts.supabase.co";
    private const string Key = "sb_publishable_qrLVNtK3nzPTUS5FzPeciQ_KKWWFdgo";

    private readonly HttpClient _client = new();

    public SupabaseService()
    {
        _client.DefaultRequestHeaders.Add("apikey", Key);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Key}");
    }

    public async Task<bool> RegisterUserAsync(User user)
    {
        var data = new
        {
            fullname = user.FullName,
            email = user.Email,
            password = user.Password,
            storename = user.StoreName ?? "",
            phone = user.Phone ?? "",
            location = user.Location ?? "",
            profilepicturepath = user.ProfilePicturePath ?? "",
            ispremium = user.IsPremium
        };

        var response = await _client.PostAsJsonAsync($"{Url}/rest/v1/users", data);
        return response.IsSuccessStatusCode;
    }

    public async Task<User?> LoginUserAsync(string email, string password)
    {
        string safeEmail = Uri.EscapeDataString(email);
        string safePassword = Uri.EscapeDataString(password);

        var response = await _client.GetAsync(
            $"{Url}/rest/v1/users?email=eq.{safeEmail}&password=eq.{safePassword}&select=*"
        );

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var arr = doc.RootElement;

        if (arr.GetArrayLength() == 0)
            return null;

        var u = arr[0];

        return new User
        {
            Id = u.GetProperty("id").GetInt32(),
            FullName = u.GetProperty("fullname").GetString() ?? "",
            Email = u.GetProperty("email").GetString() ?? "",
            Password = u.GetProperty("password").GetString() ?? "",
            StoreName = u.GetProperty("storename").GetString(),
            Phone = u.GetProperty("phone").GetString(),
            Location = u.GetProperty("location").GetString(),
            ProfilePicturePath = u.GetProperty("profilepicturepath").GetString(),
            IsPremium = u.GetProperty("ispremium").GetBoolean()
        };
    }
}