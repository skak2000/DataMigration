using DataAccessLayer.Models;
using System.Diagnostics;

namespace DataMigration
{
    public static class DataLogger
    {
        public static List<LogDataDTO> logger = new List<LogDataDTO>();

        public static void AddLog(int number, long time, Guid ModuleId, string action)
        {
            LogDataDTO log = new LogDataDTO()
            {
                Number = number,
                Time = time,
                ModuleId = ModuleId,
                Action = action
            };
            logger.Add(log);

            Console.WriteLine($"{action}: " + time);
        }

        public static DataMigrationLogger GetLastLog(Guid ModuleId, int rows)
        {
            DataMigrationLogger log = new DataMigrationLogger();

            foreach (var item in logger)
            {
                if (item.Number == 5)
                {
                    log.GetData = item.Time;
                }
                else if (item.Number == 10)
                {
                    log.CreateDto = item.Time;
                }
                else if (item.Number == 15)
                {
                    log.GetMappings = item.Time;
                }
                else if (item.Number == 20)
                {
                    log.SendData = item.Time;
                }
                else if (item.Number == 30)
                {
                    log.BulkInsertMappings = item.Time;
                }
                else if (item.Number == 32)
                {
                    log.InsertUpdateMappings = item.Time;
                }
                else if (item.Number == 35)
                {
                    log.InsertMappingTotal = item.Time;
                }
                else if (item.Number == 50)
                {
                    log.TotalTime = item.Time;
                }

            }

            log.ModuleId = ModuleId;
            log.CreateTime = DateTime.Now;
            log.RowCount = rows;
            logger.Clear();
            return log;
        }
    }

    public class LogDataDTO
    {
        public int Number { get; set; }
        public long Time { get; set; }
        public Guid ModuleId { get; set; }
        public string Action { get; set; }

    }

    //public class LogData
    //{
    //    public long GetData { get; set; }
    //    public long CreateDTO { get; set; }
    //    public long GetMappings { get; set; }
    //    public long SendData { get; set; }
    //    public long BulkInsertMappings { get; set; }
    //    public long InsertUpdateMappings { get; set; }
    //    public long InsertMapping { get; set; }
    //    public long RowCount { get; set; }
    //    public Guid ModuleId { get; set; }
    //    public DateTime CreateTime { get; set; }
    //}
}
