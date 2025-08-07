using DataAccessLayer.Data;
using DataMigration.Core.DTO;
using System.Data;

namespace DataMigration.Core
{
    public class BasicModule
    {       
        public virtual Guid TenantId { get; set; }
        public virtual Guid InstanceId { get; set; }
        public virtual int Errors { get; set; }
        public virtual Guid ModuleId { get; set; }
        public virtual string Name { get; set; }
        public virtual int PriorityLevel { get; set; }

        public void Initialize(Guid tenantId, Guid instanceId, int errors)
        {
            TenantId = tenantId;
            InstanceId = instanceId;
            Errors = errors;
        }

        public virtual string Query(DataSyncCoreContext context)
        {
            return string.Empty;
        }

        public virtual BasicDTO CreateDTO(DataTable data)
        {
            return null;
        }
        
        public virtual async Task<List<CoreIdMap>> SendData(BasicDTO input)
        {            
            return new List<CoreIdMap>();
        }

        /// <summary>
        /// Retry with half for every failure
        /// </summary>
        /// <param name="numberOfRows"></param>
        /// <returns></returns>
        public int SelectNumbers(int numberOfRows)
        {
            int divideNumber = 1;
            if (Errors > 0)
            {
                divideNumber = (int)Math.Pow(2, Errors);
            }

            int select = numberOfRows / divideNumber;

            // always return 1 rows
            return select == 0 ? 1 : select;
        }

        /// <summary>
        /// Get all mappings for one modular
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rowName"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public List<CoreIdMap> GetMappingsByColumnName(DataTable data, string rowName, Guid moduleId)
        {
            HashSet<string> traceList = new HashSet<string>();
            CoreMapping mapping = new CoreMapping(moduleId, TenantId, InstanceId);

            foreach (DataRow row in data.Rows)
            {
                traceList.Add(row[rowName].ToString());
            }

            List<CoreIdMap> mappingsList = mapping.GetMappings(traceList);

            return mappingsList;
        }

        /// <summary>
        /// Make a row not to be transfer
        /// </summary>
        /// <param name="missingValues"></param>
        public void RemoveRowFromDataset(List<CoreIdMap> missingValues)
        {
            // Remove rows with out mappings
            CoreMapping cm = new CoreMapping(ModuleId, TenantId, InstanceId);
            missingValues = missingValues.DistinctBy(x => x.TraceId).ToList();
            
            if (missingValues.Count > 0)
            {
                cm.InsertMapping(missingValues, false);
            }
        }
    }
}
