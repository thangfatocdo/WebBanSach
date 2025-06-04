using System;
using System.Collections.Generic;

namespace WebBanSach.Models.Entities;

public partial class InventoryExport
{
    public int? UserId { get; set; }

    public DateTime? ExportDate { get; set; }

    public int IepId { get; set; }

    public int? OrderId { get; set; }

    public virtual ICollection<InventoryDetail> InventoryDetails { get; set; } = new List<InventoryDetail>();

    public virtual Order? Order { get; set; }

    public virtual User? User { get; set; }
}
