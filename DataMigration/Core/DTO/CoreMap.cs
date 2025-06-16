namespace DataMigration.Core.DTO
{
    public class CoreIdMap
    {
        public CoreIdMap(string traceId, string value)
        {
            TraceId = traceId;
            Value = value;

            string[] keys = traceId.Split('_');
            Key1 = keys.First();

            if (keys.Length > 1)
            {
                Key2 = keys[1];  
            }
            if (keys.Length > 2)
            {
                Key3 = keys[2];
            }
        }

        /// <summary>
        /// SourceId
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// New public identifier
        /// </summary>
        public string Value { get; set; }


        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public string Key3 { get; set; }
    }
}
