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
    public string Status { get; set; } = "Pending";
    public DateTime? ETA { get; set; }
    public int UserId { get; set; }   
}