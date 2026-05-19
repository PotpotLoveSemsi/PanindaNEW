using SQLite;
using Paninda.Models;

namespace Paninda.Services;

public class SaleDatabase
{
    private readonly SQLiteAsyncConnection _database;

    public SaleDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<Sale>().Wait();
    }

    public Task<List<Sale>> GetSalesAsync(int userId)
    {
        return _database.Table<Sale>()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.DateSold)
            .ToListAsync();
    }

    public Task<int> AddSaleAsync(Sale sale)
    {
        return _database.InsertAsync(sale);
    }
}