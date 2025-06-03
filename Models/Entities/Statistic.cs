using System;
using System.Collections.Generic;

namespace WebBanSach.Models.Entities;

public partial class Statistic
{
    public string? Title { get; set; }

    public int? TotalBooksSold { get; set; }

    public int? TotalCustomers { get; set; }

    public int? TotalOrders { get; set; }

    public int? TotalQuantity { get; set; }

    public int? YearRevenue { get; set; }
}
