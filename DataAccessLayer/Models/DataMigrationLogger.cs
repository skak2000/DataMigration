using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class DataMigrationLogger
{
    public int Id { get; set; }

    public long GetData { get; set; }

    public long CreateDto { get; set; }

    public long GetMappings { get; set; }

    public long SendData { get; set; }

    public long BulkInsertMappings { get; set; }

    public long InsertUpdateMappings { get; set; }

    public long InsertMapping { get; set; }

    public long RowCount { get; set; }

    public Guid ModuleId { get; set; }

    public DateTime CreateTime { get; set; }
}
