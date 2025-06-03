using System;
using System.Collections.Generic;

namespace WebBanSach.Models.Entities;

public partial class BookImage
{
    public int ImageId { get; set; }

    public int? BookId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Book? Book { get; set; }
}
