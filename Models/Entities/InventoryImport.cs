using System;
using System.Collections.Generic;

namespace WebBanSach.Models.Entities;
public partial class InventoryImport
{
    public int ImportId { get; set; }

    public int? UserId { get; set; }

    public DateTime? ImportDate { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<InventoryDetail> InventoryDetails { get; set; } = new List<InventoryDetail>();

    public virtual User? User { get; set; }
}
