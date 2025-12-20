using System;
using System.Collections.Generic;

namespace DBFJoelsBokhandelLab2.Models;

public partial class OrderDetaljer
{
    public int OrderId { get; set; }

    public string Isbn { get; set; } = null!;

    public int Antal { get; set; }

    public decimal PrisVidKöp { get; set; }

    public virtual Böcker IsbnNavigation { get; set; } = null!;

    public virtual Ordrar Order { get; set; } = null!;
}
