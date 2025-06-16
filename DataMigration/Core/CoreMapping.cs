using DataMigration.Core.DTO;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace DataMigration.Core
{
    public class CoreMapping
    {
        private Guid TenantId { get; set; }
        private Guid InstanceId { get; set; }
        private Guid ModuleId { get; set; }

        public CoreMapping(Guid moduleId, Guid tenantId, Guid instanceId)
        {
            TenantId = tenantId;
            InstanceId = instanceId;
            ModuleId = moduleId;
        }

        /// <summary>
        /// Return mappings for the list of traceIds
        /// </summary>
        /// <param name="traceIds"></param>
        /// <returns></returns>
        public List<CoreIdMap> GetMappings(HashSet<string> traceIds)
        {
            List<CoreIdMap> res = new List<CoreIdMap>();
            string guidTemp = Guid.NewGuid().ToString();
            string tempTable = $"#TempGuids_{guidTemp}";
            string doneTable = $"DoneTable_{ModuleId}";

            string query = $"SELECT TraceId, Value FROM [{doneTable}] dt INNER JOIN [{tempTable}] temp ON dt.TraceId = temp.Id where dt.TenantId = '{TenantId}' AND dt.InstanceId = '{InstanceId}' AND dt.Success = 1;";

            if (AppConfig.Turbo)
            {
                string ten = TenantId.ShortGuid();
                string inst = InstanceId.ShortGuid();
                string mod = ModuleId.ShortGuid();

                doneTable = $"DoneTable_{mod}_{ten}_{inst}";
                query = $"SELECT TraceId, Value FROM [{doneTable}] dt INNER JOIN [{tempTable}] temp ON dt.TraceId = temp.Id where dt.Success = 1;";
            }


            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(string));

            foreach (string id in traceIds)
            {
                table.Rows.Add(id);
            }

            using var connection = new SqlConnection(AppConfig.ConnectionString);
            connection.Open();

            // Create temp table
            using (var cmd = new SqlCommand($"CREATE TABLE [{tempTable}] (Id VARCHAR(100) PRIMARY KEY);", connection))
            {
                cmd.ExecuteNonQuery();
            }

            using (var bulk = new SqlBulkCopy(connection))
            {
                bulk.DestinationTableName = tempTable;
                bulk.WriteToServer(table);
            }

            Stopwatch sw = Stopwatch.StartNew();

           
            // Get mappings
            using (var cmd = new SqlCommand(query, connection))
            {
                DataTable dataset = new DataTable();
                using var reader = cmd.ExecuteReader();

                dataset.Load(reader);
                res = MappingTable(dataset);
            }

            Console.WriteLine("GetMappings Time:" + sw.ElapsedMilliseconds);
            DataLogger.AddLog(15, sw.ElapsedMilliseconds, ModuleId, "GetMappings");
            return res;
        }

        /// <summary>
        /// Reset success or failed
        /// </summary>
        /// <param name="success"></param>
        public void ResetMapping(bool success)
        {
            string doneTable = $"DoneTable_{ModuleId}";

            if (AppConfig.Turbo)
            {
                string ten = TenantId.ShortGuid();
                string inst = InstanceId.ShortGuid();
                string mod = ModuleId.ShortGuid();

                doneTable = $"DoneTable_{mod}_{ten}_{inst}";               
            }

            using (SqlConnection connection = new SqlConnection(AppConfig.ConnectionString))
            {
                connection.Open();

                int value = success ? 1 : 0;

                string query = $"DELETE FROM [{doneTable}] WHERE [Success] = {value};";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Inserter data into donetable and remover it from the result
        /// </summary>
        /// <param name="res"></param>
        /// <param name="success"></param>
        public void InsertMapping(List<CoreIdMap> res, bool success)
        {
            // Get table layout
            string doneTable = $"DoneTable_{ModuleId}";

            if (AppConfig.Turbo)
            {
                string ten = TenantId.ShortGuid();
                string inst = InstanceId.ShortGuid();
                string mod = ModuleId.ShortGuid();

                doneTable = $"DoneTable_{mod}_{ten}_{inst}";
            }

            DataTable table = BulkInsert.GetDataTableLayout($"[doneTable]");


            foreach (CoreIdMap item in res)
            {
                DataRow newRow = table.NewRow();
                //newRow["Id"] = Guid.NewGuid();
                newRow["TraceId"] = item.TraceId;
                newRow["Value"] = item.Value;
                
                if (AppConfig.Turbo == false)
                {
                    newRow["TenantId"] = TenantId;
                    newRow["InstanceId"] = InstanceId;
                }

                newRow["Key1"] = item.Key1;
                newRow["Key2"] = item.Key2;
                newRow["Key3"] = item.Key3;
                newRow["success"] = success;
                table.Rows.Add(newRow);
            }

            string[] keyColumns = { "TraceId", "TenantId", "InstanceId" };
            string[] updateColumns = { "Value" };

            BulkInsert.BulkInsertUpdate(doneTable, table, keyColumns, updateColumns);
        }

        private static List<CoreIdMap> MappingTable(DataTable table)
        {
            List<CoreIdMap> res = new List<CoreIdMap>();

            foreach (DataRow row in table.Rows)
            {
                string traceId = row["TraceId"].ToString();
                string value = row["Value"].ToString();

                CoreIdMap map = new CoreIdMap(traceId, value);
                res.Add(map);
            }
            return res;
        }
    }
}
