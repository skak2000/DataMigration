using DataMigration.Core.DTO;

namespace DataMigration.Modules.Story
{
    public class StoryRequest : BasicDTO
    {
        // Title is unik for 1 Author
        public Guid AuthorId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ISBN { get; set; }
        public string TraceId { get; set; }
    }
}
