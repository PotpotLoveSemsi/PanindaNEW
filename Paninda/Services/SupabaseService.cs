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

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;

        if (arr.GetArrayLength() == 0) return null;

        return ParseUser(arr[0]);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        string safeEmail = Uri.EscapeDataString(email);

        var response = await _client.GetAsync(
            $"{Url}/rest/v1/users?email=eq.{safeEmail}&select=*"
        );

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;

        if (arr.GetArrayLength() == 0) return null;

        return ParseUser(arr[0]);
    }

    public async Task<bool> UpdateUserAsync(User user)
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

        var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{Url}/rest/v1/users?id=eq.{user.Id}"
        );

        request.Content = JsonContent.Create(data);
        request.Headers.Add("Prefer", "return=minimal");

        var response = await _client.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<string> UploadProfileImageAsync(Stream imageStream, string fileName)
    {
        string storagePath = $"profiles/{fileName}";
        string uploadUrl = $"{Url}/storage/v1/object/profile-images/{storagePath}";

        using var content = new StreamContent(imageStream);
        content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

        var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Content = content;
        request.Headers.Add("apikey", Key);
        request.Headers.Add("Authorization", $"Bearer {Key}");
        request.Headers.Add("x-upsert", "true");

        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return "";

        return $"{Url}/storage/v1/object/public/profile-images/{storagePath}";
    }

    public async Task<bool> SaveSupplierOrderAsync(SupplierOrder order)
    {
        var data = new
        {
            suppliername = order.SupplierName,
            productname = order.ProductName,
            quantity = order.Quantity,
            orderdate = order.OrderDate,
            status = order.Status,
            eta = order.ETA,
            userid = order.UserId,
            requestedprice = order.RequestedPrice,
            confirmedprice = order.ConfirmedPrice
        };

        var response = await _client.PostAsJsonAsync($"{Url}/rest/v1/supplierorders", data);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<SupplierOrder>> GetSupplierOrdersAsync(int userId)
    {
        var response = await _client.GetAsync(
            $"{Url}/rest/v1/supplierorders?userid=eq.{userId}&select=*"
        );

        if (!response.IsSuccessStatusCode)
            return new List<SupplierOrder>();

        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;

        var orders = new List<SupplierOrder>();

        foreach (var o in arr.EnumerateArray())
        {
            orders.Add(new SupplierOrder
            {
                Id = o.GetProperty("id").GetInt32(),
                SupplierName = o.GetProperty("suppliername").GetString() ?? "",
                ProductName = o.GetProperty("productname").GetString() ?? "",
                Quantity = o.GetProperty("quantity").GetInt32(),
                OrderDate = o.GetProperty("orderdate").GetDateTime(),
                Status = o.GetProperty("status").GetString() ?? "Pending",
                ETA = o.TryGetProperty("eta", out var eta) && eta.ValueKind != JsonValueKind.Null ? eta.GetDateTime() : null,
                UserId = o.GetProperty("userid").GetInt32(),
                RequestedPrice = o.TryGetProperty("requestedprice", out var rp) && rp.ValueKind != JsonValueKind.Null ? rp.GetDecimal() : null,
                ConfirmedPrice = o.TryGetProperty("confirmedprice", out var cp) && cp.ValueKind != JsonValueKind.Null ? cp.GetDecimal() : null
            });
        }

        return orders;
    }

    private User ParseUser(JsonElement u)
    {
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