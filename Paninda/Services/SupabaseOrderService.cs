using System.Net.Http.Json;
using System.Text.Json;
using Paninda.Models;

namespace Paninda.Services;

public class SupabaseOrderService
{
    private const string Url = "https://pbeihlxbxhxgehmtyjts.supabase.co";
    private const string Key = "sb_publishable_qrLVNtK3nzPTUS5FzPeciQ_KKWWFdgo";

    private readonly HttpClient _client = new();
    private List<SupplierOrder> _cache = new();

    public SupabaseOrderService()
    {
        _client.DefaultRequestHeaders.Add("apikey", Key);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Key}");
    }

    public async Task<List<SupplierOrder>> GetOrdersAsync(int userId)
    {
        if (_cache.Count > 0)
            return _cache;

        var response = await _client.GetAsync($"{Url}/rest/v1/supplierorders?userid=eq.{userId}&select=*");

        if (!response.IsSuccessStatusCode)
            return new List<SupplierOrder>();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(json).RootElement;

        _cache.Clear();

        foreach (var item in data.EnumerateArray())
        {
            _cache.Add(new SupplierOrder
            {
                Id = item.GetProperty("id").GetInt32(),
                UserId = item.GetProperty("userid").GetInt32(),
                SupplierName = item.GetProperty("suppliername").GetString() ?? "",
                Status = item.GetProperty("status").GetString() ?? "",
                ETA = item.TryGetProperty("eta", out var eta) && eta.ValueKind != JsonValueKind.Null
                    ? eta.GetDateTime()
                    : null
            });
        }

        return _cache;
    }

    public async Task<bool> AddOrderAsync(SupplierOrder order)
    {
        var data = new
        {
            userid = order.UserId,
            suppliername = order.SupplierName,
            status = order.Status,
            eta = order.ETA
        };

        var response = await _client.PostAsJsonAsync($"{Url}/rest/v1/supplierorders", data);

        if (response.IsSuccessStatusCode)
        {
            _cache.Add(order);
            return true;
        }

        return false;
    }
}