using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataMigration.Core;
using DataMigration.Core.DTO;
using DataMigration.Modules.Author;
using DataMigration.Modules.Chapter;
using DataMigration.Modules.Story;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;

namespace DataMigration
{
    internal class MigrationController
    {
        DataSyncCoreContext context = new DataSyncCoreContext();

        public async Task DoWork()
        {
            Stopwatch sw = new Stopwatch();
            var customers = context.Customers.ToList();//.Where(x => x.TenantId == Guid.Parse("2E2D2610-78F7-42F0-95C7-038276D5DEC7")).ToList();
            foreach (Customer item in customers)
            {
                Console.WriteLine("New customer start");
                sw.Start();
                await RunCustomerAsync(item.TenantId, item.InstanceId);                
                Console.WriteLine("Time: " + sw.Elapsed);
                sw.Restart();
            }
            //Guid TenantId = Guid.Parse("9B55F1B3-82BD-404A-999E-F796DE2285B3");
            //Guid InstanceId = Guid.Parse("1E7A2C6D-4602-44DC-AF8F-63C6BD85329F");          
        }

        public async Task RunCustomerAsync(Guid TenantId, Guid InstanceId)
        {
            List<BasicModule> modules = new List<BasicModule>();

            modules.Add(new AuthorModule());
            modules.Add(new StoryModule());
            modules.Add(new ChapterModule());

            modules = modules.OrderBy(x => x.PriorityLevel).ToList();
            CoreDatabase cd = new CoreDatabase(TenantId, InstanceId);

            foreach (BasicModule module in modules)
            {
                Console.WriteLine("Start: " + module.Name);
                Console.WriteLine("");
                Console.WriteLine("");
                ModuleRun? lastRun = await context.ModuleRuns.OrderBy(x => x.LastRun).FirstOrDefaultAsync(x => x.ModulId == module.ModuleId && x.TenantId == TenantId && x.InstanceId == InstanceId);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                if (lastRun == null)
                {
                    lastRun = await CreateNewRunAsync(module.ModuleId, TenantId, InstanceId);
                }
                module.Initialize(TenantId, InstanceId, lastRun.ErrorCount);

                string sqlQuery = module.Query(context);
                Console.WriteLine("Query: " + stopwatch.ElapsedMilliseconds);
                stopwatch.Restart();

                DataTable data = cd.GetData(sqlQuery, module.ModuleId);
                DataLogger.AddLog(5, stopwatch.ElapsedMilliseconds, module.ModuleId, "GetData");
                Console.WriteLine("GetData: " + stopwatch.ElapsedMilliseconds);
                stopwatch.Restart();

                bool failSafe = true;

                Stopwatch time = new Stopwatch();
                time.Start();
                while (data.Rows.Count > 0 && failSafe)
                {
                    module.Initialize(lastRun.TenantId, lastRun.InstanceId, lastRun.ErrorCount);
              
                    try
                    {
                        BasicDTO dto = module.CreateDTO(data);                      
                        DataLogger.AddLog(10, stopwatch.ElapsedMilliseconds, module.ModuleId, "CreateDTO");
                        stopwatch.Restart();

                        List<CoreIdMap> res = await module.SendData(dto);                 
                        DataLogger.AddLog(20, stopwatch.ElapsedMilliseconds, module.ModuleId, "SendData");
                        stopwatch.Restart();

                        CoreMapping cm = new CoreMapping(module.ModuleId, module.TenantId, module.InstanceId);
                        cm.InsertMapping(res, true);
                        DataLogger.AddLog(35, stopwatch.ElapsedMilliseconds, module.ModuleId, "InsertMapping Total");

                        lastRun = HandleSuccess(lastRun, res.Count, sqlQuery, time.ElapsedMilliseconds);                        
                    }
                    catch (Exception ex)
                    {
                        lastRun.RunTimeMs = time.ElapsedMilliseconds;
                        lastRun = HandleFailure(module, lastRun, data, ex.Message + "" +  ex.StackTrace, time.ElapsedMilliseconds);                        
                    }


                    DataLogger.AddLog(50, time.ElapsedMilliseconds, module.ModuleId, "Total time");

                    sqlQuery = module.Query(context);
                                       
                    Console.WriteLine("");
                    Console.WriteLine("");
                    DataMigrationLogger log = DataLogger.GetLastLog(module.ModuleId, data.Rows.Count);
                    context.DataMigrationLoggers.Add(log);
                    context.SaveChanges();
                    time.Restart();

                    stopwatch.Restart();
                    data = cd.GetData(sqlQuery, module.ModuleId);
                    Console.WriteLine("Rows: " + data.Rows.Count);
                    DataLogger.AddLog(5, stopwatch.ElapsedMilliseconds, module.ModuleId, "GetData");
                    stopwatch.Restart();

                   
                    failSafe = data.Rows.Count > 0;
                }
            }
        }



        private ModuleRun HandleSuccess(ModuleRun lastRun, int rowCount, string sqlQuery, long runTimeMs)
        {          
            ModuleRun newRun = new ModuleRun()
            {
                ErrorCount = lastRun.ErrorCount,
                InstanceId = lastRun.InstanceId,
                TenantId = lastRun.TenantId,
                ModulId = lastRun.ModulId,
                LastRun = DateTime.Now,
                SqlQuery = "sqlQuery",
                RunTimeMs = runTimeMs,
                Success = true,
                RowCount = rowCount
            };

            if (newRun.ErrorCount > 0)
            {
                newRun.ErrorCount--;
            }

            context.ModuleRuns.Add(newRun);

            return newRun;
        }

        private ModuleRun HandleFailure(BasicModule module, ModuleRun lastRun, DataTable data, string sqlQuery, long runTimeMs)
        {
            ModuleRun newRun = new ModuleRun()
            {
                ErrorCount = lastRun.ErrorCount,
                InstanceId = lastRun.InstanceId,
                TenantId = lastRun.TenantId,
                ModulId = lastRun.ModulId,
                LastRun = DateTime.Now,
                SqlQuery = sqlQuery,
                RunTimeMs = runTimeMs,
                Success = false,
                RowCount = data.Rows.Count
            };

            if (data.Rows.Count != 1)
            {
                newRun.ErrorCount++;
            }
            else
            {
                // Remove problem row
                foreach (DataRow row in data.Rows)
                {
                    var traceId = row["TraceId"].ToString();
                    var error = new CoreIdMap(traceId, Guid.Empty.ToString());
                    var errorMapping = new CoreMapping(module.ModuleId, module.TenantId, module.InstanceId);
                    errorMapping.InsertMapping(new List<CoreIdMap> { error }, false);
                }
            }
            context.ModuleRuns.Add(newRun);
            return newRun;
        }

        public void ResetFailRows()
        {
            List<BasicModule> modules = new List<BasicModule>();

            modules.Add(new AuthorModule());
            modules.Add(new StoryModule());
            modules.Add(new ChapterModule());

            foreach (BasicModule module in modules)
            {
                CoreMapping cm = new CoreMapping(module.ModuleId, module.TenantId, module.InstanceId);
                cm.ResetMapping(false);
            }
        }

        public async Task<ModuleRun> CreateNewRunAsync(Guid moduleId, Guid tenantId, Guid instanceId)
        {
            ModuleRun lastRun = new ModuleRun()
            {
                ModulId = moduleId,
                ErrorCount = 0,
                InstanceId = instanceId,
                TenantId = tenantId,
                LastRun = DateTime.Now,
                RowCount = 0,
                SqlQuery = string.Empty,
                Success = false
            };
            await context.ModuleRuns.AddAsync(lastRun);
            await context.SaveChangesAsync();

            return lastRun;
        }
    }
}
