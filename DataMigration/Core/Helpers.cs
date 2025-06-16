namespace DataMigration.Core
{
    internal static class Helpers
    {
        public static bool IsSafeName(string name)
        {
            return name.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-') && !string.IsNullOrWhiteSpace(name);
        }

        public static void ValidateName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName) || !IsSafeName(tableName) || tableName.Contains("--"))
            {
                throw new ArgumentException("Invalid table name.");
            }
        }

        public static void ValidateColumnNames(string[] columns)
        {
            foreach (var column in columns)
            {
                if (!IsSafeName(column))
                {
                    throw new ArgumentException($"Invalid column name: {column}");
                }
            }
        }

        public static string ShortGuid(this Guid input)
        {
            string first8 = input.ToString().Substring(0, 8);
            return first8;
        }
    }
}
