using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class DoneTable
{
    public string TraceId { get; set; } = null!;

    public string? Value { get; set; }

    public bool Success { get; set; }

    public int? Key1 { get; set; }

    public int? Key2 { get; set; }

    public int? Key3 { get; set; }
}
