using System;
using System.Collections.Generic;

namespace WebBanSach.Models.Entities;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public int? CustomerId { get; set; }

    public int? BookId { get; set; }

    public int? Quantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Book? Book { get; set; }

    public virtual Customer? Customer { get; set; }
}
