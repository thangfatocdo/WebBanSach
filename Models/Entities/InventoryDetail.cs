using System;
using System.Collections.Generic;

namespace WebBanSach.Models.Entities;

public partial class InventoryDetail
{
    public int DetailId { get; set; }

    public int? ImportId { get; set; }

    public int? BookId { get; set; }

    public int Quantity { get; set; }

    public int? IepId { get; set; }

    public string? Type { get; set; }

    public virtual Book? Book { get; set; }

    public virtual InventoryExport? Iep { get; set; }

    public virtual InventoryImport? Import { get; set; }
}
