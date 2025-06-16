using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Customer
{
    public string Name { get; set; } = null!;

    public Guid TenantId { get; set; }

    public Guid InstanceId { get; set; }
}
