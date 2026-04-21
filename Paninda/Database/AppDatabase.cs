using SQLite;
using Paninda.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Paninda;

public class AppDatabase
{
    private readonly SQLiteAsyncConnection _database;

    public AppDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<User>().Wait();
        _database.CreateTableAsync<Product>().Wait();
        _database.CreateTableAsync<SupplierOrder>().Wait();
    }

    // User methods
    public Task<int> SaveUserAsync(User user) => _database.InsertAsync(user);
    public Task<User> GetUserByEmailAsync(string email) => _database.Table<User>().FirstOrDefaultAsync(u => u.Email == email);
    public Task<User> GetUserByEmailAndPasswordAsync(string email, string password) => _database.Table<User>().FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
    public Task<List<User>> GetAllUsersAsync() => _database.Table<User>().ToListAsync();
    public Task<int> UpdateUserAsync(User user) => _database.UpdateAsync(user);

    // Product methods (now filtered by user ID)
    public Task<List<Product>> GetProductsAsync(int userId) => _database.Table<Product>().Where(p => p.UserId == userId).ToListAsync();
    public Task<int> SaveProductAsync(Product product) => _database.InsertAsync(product);
    public Task<int> UpdateProductAsync(Product product) => _database.UpdateAsync(product);
    public Task<int> DeleteProductAsync(Product product) => _database.DeleteAsync(product);

    // SupplierOrder methods
    public Task<List<SupplierOrder>> GetSupplierOrdersAsync() => _database.Table<SupplierOrder>().ToListAsync();
    public Task<int> SaveSupplierOrderAsync(SupplierOrder order) => _database.InsertAsync(order);
}