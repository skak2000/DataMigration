using DataMigration.Core.DTO;

namespace DataMigration.Modules.Chapter
{
    public class ChapterRequest : BasicDTO
    {
        public Guid BookId { get; set; }
        public string Title { get; set; }
        public int ChapterNumber { get; set; }
        public string Text { get; set; }
        public string TraceId { get; set; }
    }
}
