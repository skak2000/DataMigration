using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class ModuleRun
{
    public int Id { get; set; }

    public Guid ModulId { get; set; }

    public DateTime LastRun { get; set; }

    public bool Success { get; set; }

    public int ErrorCount { get; set; }

    public int RowCount { get; set; }

    public string? SqlQuery { get; set; }

    public long? RunTimeMs { get; set; }

    public Guid TenantId { get; set; }

    public Guid InstanceId { get; set; }
}
