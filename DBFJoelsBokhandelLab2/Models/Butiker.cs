using System;
using System.Collections.Generic;

namespace DBFJoelsBokhandelLab2.Models;

public partial class Butiker
{
    public int ButikId { get; set; }

    public string? ButiksNamn { get; set; }

    public string? Adress { get; set; }

    public virtual ICollection<LagerSaldo> LagerSaldos { get; set; } = new List<LagerSaldo>();

    public virtual ICollection<Ordrar> Ordrars { get; set; } = new List<Ordrar>();
}
