using System;
using System.Collections.Generic;

namespace WebBanSach.Models.Entities;

public partial class BookRating
{
    public int RatingId { get; set; }

    public int CustomerId { get; set; }

    public int BookId { get; set; }

    public int? RatingValue { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;
}
