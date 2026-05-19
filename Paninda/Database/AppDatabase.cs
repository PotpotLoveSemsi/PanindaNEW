using SQLite;
using Paninda.Models;

namespace Paninda.Database;

public class AppDatabase
{
    private readonly SQLiteAsyncConnection _database;

    public AppDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);

        _database.CreateTableAsync<User>().Wait();
        _database.CreateTableAsync<Product>().Wait();
        _database.CreateTableAsync<SupplierOrder>().Wait();
        _database.CreateTableAsync<Sale>().Wait();
    }

    public Task<List<Product>> GetProductsAsync(int userId)
    {
        return _database.Table<Product>()
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> SaveProductAsync(Product product)
    {
        return await _database.InsertAsync(product) > 0;
    }

    public Task<int> UpdateProductAsync(Product product)
    {
        return _database.UpdateAsync(product);
    }

    public Task<int> DeleteProductAsync(Product product)
    {
        return _database.DeleteAsync(product);
    }

    public Task<int> UpdateUserAsync(User user)
    {
        return _database.UpdateAsync(user);
    }

    public Task<List<SupplierOrder>> GetSupplierOrdersAsync(int userId)
    {
        return _database.Table<SupplierOrder>()
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public Task<int> SaveSupplierOrderAsync(SupplierOrder order)
    {
        return _database.InsertAsync(order);
    }

    public Task<List<Sale>> GetSalesAsync(int userId)
    {
        return _database.Table<Sale>()
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public Task<int> SaveSaleAsync(Sale sale)
    {
        return _database.InsertAsync(sale);
    }
}