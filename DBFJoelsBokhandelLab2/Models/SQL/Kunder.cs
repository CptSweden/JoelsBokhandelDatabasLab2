using System;
using System.Collections.Generic;

namespace DBFJoelsBokhandelLab2.Models;

public partial class Kunder
{
    public int KundId { get; set; }

    public string Namn { get; set; } = null!;

    public string Adress { get; set; } = null!;

    public string? Ort { get; set; }

    public string? PostNummer { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Ordrar> Ordrars { get; set; } = new List<Ordrar>();
}
