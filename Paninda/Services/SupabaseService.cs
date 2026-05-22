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

        var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
        request.Content = content;
        request.Headers.Add("x-upsert", "true");

        var response = await _client.SendAsync(request);

        return response.IsSuccessStatusCode
            ? $"{Url}/storage/v1/object/public/profile-images/{storagePath}"
            : "";
    }

    public async Task<bool> SaveProductAsync(Product product)
    {
        var data = new
        {
            userid = product.UserId,
            name = product.Name,
            category = product.Category,
            price = product.Price,
            costprice = product.CostPrice,
            stock = product.Stock,
            quantity = product.Quantity,
            minstocklevel = product.MinStockLevel,
            soldtoday = product.SoldToday,
            lastsolddate = product.LastSoldDate,
            dateadded = product.DateAdded
        };

        var response = await _client.PostAsJsonAsync($"{Url}/rest/v1/products", data);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        var data = new
        {
            stock = product.Stock,
            quantity = product.Quantity,
            soldtoday = product.SoldToday,
            lastsolddate = product.LastSoldDate
        };

        var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{Url}/rest/v1/products?id=eq.{product.Id}"
        );

        request.Content = JsonContent.Create(data);
        request.Headers.Add("Prefer", "return=minimal");

        var response = await _client.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Product>> GetProductsAsync(int userId)
    {
        var response = await _client.GetAsync(
            $"{Url}/rest/v1/products?userid=eq.{userId}&select=*"
        );

        if (!response.IsSuccessStatusCode)
            return new List<Product>();

        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;

        var products = new List<Product>();

        foreach (var p in arr.EnumerateArray())
        {
            products.Add(new Product
            {
                Id = p.GetProperty("id").GetInt32(),
                UserId = p.GetProperty("userid").GetInt32(),
                Name = p.GetProperty("name").GetString() ?? "",
                Category = p.GetProperty("category").GetString() ?? "General",
                Price = p.GetProperty("price").GetDecimal(),
                CostPrice = p.GetProperty("costprice").GetDecimal(),
                Stock = p.GetProperty("stock").GetInt32(),
                Quantity = p.GetProperty("quantity").GetInt32(),
                MinStockLevel = p.GetProperty("minstocklevel").GetInt32(),
                SoldToday = p.GetProperty("soldtoday").GetInt32(),
                LastSoldDate = p.GetProperty("lastsolddate").GetDateTime(),
                DateAdded = p.GetProperty("dateadded").GetDateTime()
            });
        }

        return products;
    }

    public async Task<bool> SaveSaleAsync(Sale sale)
    {
        var data = new
        {
            userid = sale.UserId,
            productname = sale.ProductName,
            quantity = sale.Quantity,
            totalprice = sale.TotalPrice,
            profit = sale.Profit,
            datesold = sale.DateSold
        };

        var response = await _client.PostAsJsonAsync($"{Url}/rest/v1/sales", data);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            await Application.Current.MainPage.DisplayAlert("Supabase Error", error, "OK");
            return false;
        }

        return true;
    }

    public async Task<List<Sale>> GetSalesAsync(int userId)
    {
        var response = await _client.GetAsync(
            $"{Url}/rest/v1/sales?userid=eq.{userId}&select=*"
        );

        if (!response.IsSuccessStatusCode)
            return new List<Sale>();

        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;

        var sales = new List<Sale>();

        foreach (var s in arr.EnumerateArray())
        {
            sales.Add(new Sale
            {
                Id = s.GetProperty("id").GetInt32(),
                UserId = s.GetProperty("userid").GetInt32(),
                ProductName = s.GetProperty("productname").GetString() ?? "",
                Quantity = s.GetProperty("quantity").GetInt32(),
                TotalPrice = s.GetProperty("totalprice").GetDecimal(),
                Profit = s.GetProperty("profit").GetDecimal(),
                DateSold = s.GetProperty("datesold").GetDateTime()
            });
        }

        return sales;
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

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            await Application.Current.MainPage.DisplayAlert("Supplier Order Error", error, "OK");
            return false;
        }

        return true;
    }

    public async Task<List<SupplierOrder>> GetSupplierOrdersAsync(int userId)
    {
        var response = await _client.GetAsync(
            $"{Url}/rest/v1/supplierorders?select=*"
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            await Application.Current.MainPage.DisplayAlert("Load Orders Error", error, "OK");
            return new List<SupplierOrder>();
        }

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
                ETA = o.TryGetProperty("eta", out var eta) && eta.ValueKind != JsonValueKind.Null
                    ? eta.GetDateTime()
                    : null,
                UserId = o.GetProperty("userid").GetInt32()
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