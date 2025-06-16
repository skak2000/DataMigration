using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class DoneTable
{
    public string TraceId { get; set; } = null!;

    public string? Value { get; set; }

    public Guid TenantId { get; set; }

    public Guid InstanceId { get; set; }

    public bool Success { get; set; }

    public string? Key1 { get; set; }

    public string? Key2 { get; set; }

    public string? Key3 { get; set; }
}
