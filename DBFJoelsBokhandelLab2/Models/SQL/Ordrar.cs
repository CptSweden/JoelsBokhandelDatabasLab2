using System;
using System.Collections.Generic;

namespace DBFJoelsBokhandelLab2.Models;

public partial class Ordrar
{
    public int OrderId { get; set; }

    public int KundId { get; set; }

    public int ButikId { get; set; }

    public DateTime OrderDatum { get; set; }

    public decimal? TotalPris { get; set; }

    public virtual Butiker Butik { get; set; } = null!;

    public virtual Kunder Kund { get; set; } = null!;

    public virtual ICollection<OrderDetaljer> OrderDetaljers { get; set; } = new List<OrderDetaljer>();
}
