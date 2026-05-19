using SQLite;

namespace Paninda.Models;

public class Product
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int MinStockLevel { get; set; } = 5;
    public int SoldToday { get; set; }
    public DateTime LastSoldDate { get; set; } = DateTime.Today;
    public int UserId { get; set; }
}