using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlTypes;

namespace DataMigration.Core
{
    internal class CoreDatabase
    {
        private Guid TenantId;
        private Guid Instance;

        public CoreDatabase(Guid tenantId, Guid instance)
        {
            TenantId = tenantId;
            Instance = instance;
        }

        public DataTable GetData(string query, Guid moduleId)
        {
            query = ReplaceTableNames(query, moduleId);
            CreateDoneTableForModule(moduleId);
            DataTable res = GetDatabaseTable(query);
            return res;
        }

        /// <summary>
        /// Create a DoneTable for a module
        /// </summary>
        /// <param name="moduleId"></param>
        private void CreateDoneTableForModule(Guid moduleId)
        {
            string tableName = $"DoneTable_{moduleId}";

            string sqlCreateDoneTable = $"CREATE TABLE [{tableName}] ([TraceId] VARCHAR(100), [Value] VARCHAR(100), [TenantId] UNIQUEIDENTIFIER, [InstanceId] UNIQUEIDENTIFIER, [Success] [bit], [Key1] VARCHAR(50), [Key2] VARCHAR(50), [Key3] VARCHAR(50))";

            if (AppConfig.Turbo)
            {
                string ten = TenantId.ShortGuid();
                string inst = Instance.ShortGuid();
                string mod = moduleId.ShortGuid();

                tableName = $"DoneTable_{mod}_{ten}_{inst}";

                sqlCreateDoneTable = $"CREATE TABLE [{tableName}] ([TraceId] VARCHAR(100), [Value] VARCHAR(100), [Success] [bit], [Key1] VARCHAR(50), [Key2] VARCHAR(50), [Key3] VARCHAR(50))";
            }

            string query = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' and TABLE_NAME like '{tableName}'";

            // CreateTable 
            using (SqlConnection connection = new SqlConnection(AppConfig.ConnectionString))
            {
                connection.Open();

                using (SqlCommand checkCmd = new SqlCommand(query, connection))
                {
                    var exists = checkCmd.ExecuteScalar();

                    if (exists == null)
                    {
                        string sqlCreateDoneTableOld = $"CREATE TABLE [DoneTable_{moduleId}] ([TraceId] VARCHAR(255), [Value] VARCHAR(255), [TenantId] UNIQUEIDENTIFIER, [InstanceId] UNIQUEIDENTIFIER, [Success] [bit], PRIMARY KEY (TraceId, InstanceId, TenantId), [Key1] VARCHAR(255), [Key2] VARCHAR(255), [Key3] VARCHAR(255))";
                        using (SqlCommand createCmd = new SqlCommand(sqlCreateDoneTable, connection))
                        {
                            createCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replace table name to get customer table
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        private string ReplaceTableNames(string sql, Guid moduleId)
        {
            // Insert all used table her:
            sql = sql.Replace("[StatusOnline_Author]", $"[StatusOnlineAuthor_{TenantId}_{Instance}]");
            sql = sql.Replace("[StatusOnline_Story]", $"[StatusOnlineStory_{TenantId}_{Instance}]");
            sql = sql.Replace("[StatusOnline_Chapter]", $"[StatusOnlineChapter_{TenantId}_{Instance}]");

            if (AppConfig.Turbo)
            {
                string ten = TenantId.ShortGuid();
                string inst = Instance.ShortGuid();
                string mod = moduleId.ShortGuid();

                sql = sql.Replace("[DoneTable]", $"[DoneTable_{mod}_{ten}_{inst}]");
            }
            else
            { 
                sql = sql.Replace("[DoneTable]", $"[DoneTable_{moduleId}]");
            }
               
            return sql;
        }

        /// <summary>
        /// Return a datatable mase on the select query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private DataTable GetDatabaseTable(string query)
        {
            DataTable table = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(AppConfig.ConnectionString))
                {
                    connection.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.SelectCommand.CommandTimeout = 60;
                        adapter.Fill(table);
                    }
                }
            }
            catch (Exception es)
            {

                throw;
            }
            return table;
        }
    }
}
