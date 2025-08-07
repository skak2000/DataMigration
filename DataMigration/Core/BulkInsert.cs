using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace DataMigration.Core
{
    internal class BulkInsert
    {
        /// <summary>
        /// Insert and Update dataset, with help of a temptable for better performance 
        /// </summary>
        /// <param name="tableName">Destination table to insert data into</param>
        /// <param name="dataInput">Dataset</param>
        /// <param name="keyColumns">What keys use to id the uniq rows</param>
        /// <param name="updateColumns">What columns to update</param>
        internal static void BulkInsertUpdate(string tableName, DataTable dataInput, string[] keyColumns, string[] updateColumns)
        {
            // Protect the database against SQL Injections
            Helpers.ValidateName(tableName);
            Helpers.ValidateColumnNames(keyColumns);
            Helpers.ValidateColumnNames(updateColumns);

            string tableId = Guid.NewGuid().ToString("N");
            string stagingTableName = $"[#staging_{tableId}_{tableName}]";

            // We don't trust input datatable, we get the schema from the database
            DataTable schemaTable = GetDataTableLayout($"[{tableName}]");
            string sqlTempTable = PrepareTempTable(schemaTable, stagingTableName);

            using (SqlConnection connection = new SqlConnection(AppConfig.ConnectionString))
            {
                connection.Open();

                // Create temp table
                using (SqlCommand cmd = new SqlCommand(sqlTempTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                

                // Bulk insert temp table
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = stagingTableName;
                    bulkCopy.WriteToServer(dataInput);
                }

                DataLogger.AddLog(30, stopwatch.ElapsedMilliseconds, Guid.Empty, "BulkInsert DoneTable");
                stopwatch.Restart();

                updateColumns = RemoveProtectedColumns(updateColumns);
                // Merge data Insert/Update
                string[] allColumns = schemaTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();

                // Only correct column names is allowed in the query, remove columns that is not the database table.
                string[] keyColumnsSafe = keyColumns.Intersect(allColumns).ToArray();
                string[] updateColumnsSafe = updateColumns.Intersect(allColumns).ToArray();

                string onClause = string.Join(" AND ", keyColumnsSafe.Select(c => $"target.[{c}] = source.[{c}]"));
                string updateClause = string.Join(", ", updateColumnsSafe.Select(c => $"target.[{c}] = source.[{c}]"));
                string insertColumns = string.Join(", ", allColumns);
                string insertValues = string.Join(", ", allColumns.Select(c => $"source.[{c}]"));

                string mergeSql = $@"MERGE INTO [{tableName}] AS target
                                    USING {stagingTableName} AS source
                                    ON {onClause}
                                    WHEN MATCHED THEN
                                        UPDATE SET {updateClause}
                                    WHEN NOT MATCHED BY TARGET THEN
                                        INSERT ({insertColumns})
                                        VALUES ({insertValues});";

                using (SqlCommand command = new SqlCommand(mergeSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                DataLogger.AddLog(32, stopwatch.ElapsedMilliseconds, Guid.Empty, "InsertUpdate mappings");
                stopwatch.Restart();
            }
        }

        /// <summary>
        /// Prepa
        /// </summary>
        /// <param name="schemaTable"></param>
        /// <param name="stagingTable"></param>
        /// <returns></returns>
        private static string PrepareTempTable(DataTable schemaTable, string stagingTable)
        {
            List<string> columnDefinitions = new List<string>();

            foreach (DataColumn column in schemaTable.Columns)
            {
                string sqlType = GetSqlTypeFromDataColumn(column);
                string nullability = column.AllowDBNull ? "NULL" : "NOT NULL";
                string definition = $"{column.ColumnName} {sqlType} {nullability}";

                if (column.Unique && schemaTable.PrimaryKey.Contains(column))
                {
                    definition += " PRIMARY KEY";
                }
                columnDefinitions.Add(definition);
            }

            string sql = $"CREATE TABLE {stagingTable} ({string.Join(", ", columnDefinitions)});";
            return sql;
        }

        public static DataTable GetDataTableLayout(string tableName)
        {
            //Helpers.ValidateName(tableName);

            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(AppConfig.ConnectionString))
            {
                connection.Open();
                // Select * is not a good thing, but in this cases is is very usefull to make the code dynamic/reusable 
                // We get the tabel layout for our DataTable
                string query = $"SELECT TOP 0 * FROM {tableName};";
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    adapter.Fill(table);
                }
            }
            return table;
        }

        private static string GetSqlTypeFromDataColumn(DataColumn column)
        {
            Type dataType = column.DataType;

            if (dataType == typeof(string))
            {
                int maxLength = column.MaxLength;
                return maxLength > 0 ? $"NVARCHAR({maxLength})" : "NVARCHAR(MAX)";
            }

            if (dataType == typeof(int)) return "INT";
            if (dataType == typeof(Guid)) return "UNIQUEIDENTIFIER";
            if (dataType == typeof(DateTime)) return "DATETIME";
            if (dataType == typeof(bool)) return "BIT";
            if (dataType == typeof(decimal)) return "DECIMAL(18, 2)";
            if (dataType == typeof(double)) return "FLOAT";
            if (dataType == typeof(byte[])) return "VARBINARY(MAX)";

            throw new NotSupportedException($"Unsupported data type: {dataType.Name}");
        }

        /// <summary>
        /// Remove Columns that cannot be updated
        /// </summary>
        /// <param name="updateColumns"></param>
        /// <returns></returns>
        private static string[] RemoveProtectedColumns(string[] updateColumns)
        {
            List<string> columns = updateColumns.ToList();
            columns.Remove("PublicId");
            columns.Remove("Id");
            return columns.ToArray();
        }
    }
}
