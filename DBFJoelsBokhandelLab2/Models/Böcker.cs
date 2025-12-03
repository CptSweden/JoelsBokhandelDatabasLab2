using System;
using System.Collections.Generic;

namespace DBFJoelsBokhandelLab2.Models;

public partial class Böcker
{
    public string Isbn13 { get; set; } = null!;

    public string? Titel { get; set; }

    public string? Språk { get; set; }

    public string? Genre { get; set; }

    public decimal? Pris { get; set; }

    public DateOnly? Utgivningsdatum { get; set; }

    public int? FörfattareId { get; set; }

    public virtual Författare? Författare { get; set; }

    public virtual ICollection<LagerSaldo> LagerSaldos { get; set; } = new List<LagerSaldo>();

    public virtual ICollection<OrderDetaljer> OrderDetaljers { get; set; } = new List<OrderDetaljer>();
}
