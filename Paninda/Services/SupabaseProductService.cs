using System.Net.Http.Json;
using System.Text.Json;
using Paninda.Models;

namespace Paninda.Services;

public class SupabaseProductService
{
    private const string Url = "https://pbeihlxbxhxgehmtyjts.supabase.co";
    private const string Key = "sb_publishable_qrLVNtK3nzPTUS5FzPeciQ_KKWWFdgo";

    private readonly HttpClient _client = new();
    private List<Product> _cache = new();

    public SupabaseProductService()
    {
        _client.DefaultRequestHeaders.Add("apikey", Key);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Key}");
    }

    public async Task<List<Product>> GetProductsAsync(int userId)
    {
        if (_cache.Count > 0)
            return _cache;

        var res = await _client.GetAsync($"{Url}/rest/v1/products?userid=eq.{userId}&select=*");

        if (!res.IsSuccessStatusCode)
            return new List<Product>();

        var json = await res.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(json).RootElement;

        _cache.Clear();

        foreach (var item in data.EnumerateArray())
        {
            _cache.Add(new Product
            {
                Id = item.GetProperty("id").GetInt32(),
                UserId = item.GetProperty("userid").GetInt32(),
                Name = item.GetProperty("name").GetString() ?? "",
                Stock = item.GetProperty("stock").GetInt32(),
                MinStockLevel = item.GetProperty("minstocklevel").GetInt32(),
                SoldToday = item.GetProperty("soldtoday").GetInt32()
            });
        }

        return _cache;
    }

    // ✅ UPDATE PRODUCT
    public async Task<bool> UpdateProductAsync(Product product)
    {
        var data = new
        {
            name = product.Name,
            stock = product.Stock,
            minstocklevel = product.MinStockLevel,
            soldtoday = product.SoldToday
        };

        var res = await _client.PatchAsJsonAsync(
            $"{Url}/rest/v1/products?id=eq.{product.Id}", data);

        if (res.IsSuccessStatusCode)
        {
            var existing = _cache.FirstOrDefault(p => p.Id == product.Id);
            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Stock = product.Stock;
                existing.MinStockLevel = product.MinStockLevel;
                existing.SoldToday = product.SoldToday;
            }
            return true;
        }

        return false;
    }

    // ✅ DELETE PRODUCT
    public async Task<bool> DeleteProductAsync(int productId)
    {
        var res = await _client.DeleteAsync(
            $"{Url}/rest/v1/products?id=eq.{productId}");

        if (res.IsSuccessStatusCode)
        {
            _cache.RemoveAll(p => p.Id == productId);
            return true;
        }

        return false;
    }

    // ADD PRODUCT (unchanged)
    public async Task<bool> SaveProductAsync(Product product)
    {
        var data = new
        {
            userid = product.UserId,
            name = product.Name,
            stock = product.Stock,
            minstocklevel = product.MinStockLevel,
            soldtoday = product.SoldToday
        };

        var res = await _client.PostAsJsonAsync($"{Url}/rest/v1/products", data);

        if (res.IsSuccessStatusCode)
        {
            _cache.Add(product);
            return true;
        }

        return false;
    }
}