using System;
using System.Collections.Generic;

namespace DBFJoelsBokhandelLab2.Models;

public partial class Författare
{
    public int FörfattareId { get; set; }

    public string? Förnamn { get; set; }

    public string? Efternamn { get; set; }

    public DateOnly? Födelsedatum { get; set; }

    public virtual ICollection<Böcker> Böckers { get; set; } = new List<Böcker>();
}
