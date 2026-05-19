using SQLite;

namespace Paninda.Models;

public class Product
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }
    public int Quantity { get; set; }
    public int MinStockLevel { get; set; }

    public int SoldToday { get; set; }
    public DateTime LastSoldDate { get; set; } = DateTime.Today;

    public DateTime DateAdded { get; set; } = DateTime.Now;
}