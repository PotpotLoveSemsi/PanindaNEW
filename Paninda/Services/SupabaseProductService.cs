using Paninda.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Paninda.Services;

public class SupabaseProductService
{
    private const string Url = "https://pbeihlxbxhxgehmtyjts.supabase.co";
    private const string Key = "sb_publishable_qrLVNtK3nzPTUS5FzPeciQ_KKWWFdgo";

    private readonly HttpClient _client = new();

    public SupabaseProductService()
    {
        _client.DefaultRequestHeaders.Add("apikey", Key);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Key}");
    }

    public async Task<List<Product>> GetProductsAsync(int userId)
    {
        var res = await _client.GetAsync($"{Url}/rest/v1/products?userid=eq.{userId}&select=*");

        if (!res.IsSuccessStatusCode)
            return new List<Product>();

        var json = await res.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(json).RootElement;

        var products = new List<Product>();

        foreach (var item in data.EnumerateArray())
        {
            products.Add(new Product
            {
                Id = item.GetProperty("id").GetInt32(),
                UserId = item.GetProperty("userid").GetInt32(),
                Name = item.GetProperty("name").GetString() ?? "",
                Stock = item.GetProperty("stock").GetInt32(),
                MinStockLevel = item.GetProperty("minstocklevel").GetInt32(),
                SoldToday = item.GetProperty("soldtoday").GetInt32()
            });
        }

        return products;
    }

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
        return res.IsSuccessStatusCode;
    }

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
            $"{Url}/rest/v1/products?id=eq.{product.Id}&userid=eq.{product.UserId}",
            data);

        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProductAsync(int productId, int userId)
    {
        var res = await _client.DeleteAsync(
            $"{Url}/rest/v1/products?id=eq.{productId}&userid=eq.{userId}");

        return res.IsSuccessStatusCode;
    }
}