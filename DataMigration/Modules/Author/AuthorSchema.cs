using DataMigration.Core.DTO;

namespace DataMigration.Modules.Author
{
    public class AuthorSchema : BasicDTO
    {
        public AuthorSchema()
        {
            DataList = new List<AuthorRequest>();
        }

        public List<AuthorRequest> DataList {  get; set; }
    }
}
