﻿using System;
using System.Collections.Generic;

namespace WebBanSach.Models.Entities;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int? OrderId { get; set; }

    public int? BookId { get; set; }

    public decimal? BookPrice { get; set; }

    public int? BookQuantity { get; set; }

    public virtual Book? Book { get; set; }

    public virtual Order? Order { get; set; }
}
