using SQLite;

namespace Paninda.Models;

public class Sale
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    // ✅ Accurate profit per sale
    public decimal Profit { get; set; }

    public DateTime DateSold { get; set; }

    public int UserId { get; set; }
}