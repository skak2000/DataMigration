using DataMigration.Core.DTO;
using DataMigration.Modules.Chapter;

namespace DataMigration.Modules.Author
{
    public class ChapterSchema : BasicDTO
    {
        public ChapterSchema()
        {
            DataList = new List<ChapterRequest>();
        }

        public List<ChapterRequest> DataList {  get; set; }
    }
}
