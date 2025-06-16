using DataMigration.Core.DTO;
using DataMigration.Modules.Story;

namespace DataMigration.Modules.Author
{
    public class StorySchema : BasicDTO
    {
        public StorySchema()
        {
            DataList = new List<StoryRequest>();
        }

        public List<StoryRequest> DataList {  get; set; }
    }
}
