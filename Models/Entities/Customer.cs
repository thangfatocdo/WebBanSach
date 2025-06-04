using System;
using System.Collections.Generic;

namespace WebBanSach.Models.Entities;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<BookRating> BookRatings { get; set; } = new List<BookRating>();

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
