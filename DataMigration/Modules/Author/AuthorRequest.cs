using DataMigration.Core.DTO;

namespace DataMigration.Modules.Author
{
    public class AuthorRequest : BasicDTO
    {
        public string AuthorName { get; set; }
        public string TraceId { get; set; }
    }
}
