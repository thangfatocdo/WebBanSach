using System;
using System.Collections.Generic;

namespace WebBanSach.Models.Entities;

public partial class Order
{
    public int? CustomerId { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? ReceiveDate { get; set; }

    public decimal TotalPrice { get; set; }

    public int OrderId { get; set; }

    public int? PaymentMethodId { get; set; }

    public int? StatusId { get; set; }

    public string? Address { get; set; }

    public string? CustomerName { get; set; }

    public string? Phone { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<InventoryExport> InventoryExports { get; set; } = new List<InventoryExport>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual PaymentMethod? PaymentMethod { get; set; }

    public virtual OrderStatus? Status { get; set; }
}
