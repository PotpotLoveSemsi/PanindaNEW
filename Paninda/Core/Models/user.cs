using SQLite;
using System;

namespace Paninda.Models;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    public string? StoreName { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? ProfilePicturePath { get; set; }

    public bool IsPremium { get; set; }
}