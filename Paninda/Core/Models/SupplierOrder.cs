using SQLite;
using System;

namespace Paninda.Models;

public class SupplierOrder
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "Pending";   // Pending, PriceRequested, Confirmed
    public DateTime? ETA { get; set; }
    public int UserId { get; set; }
    public decimal? RequestedPrice { get; set; }      // price quoted by supplier (after notification)
    public decimal? ConfirmedPrice { get; set; }      // final price after client confirms
}