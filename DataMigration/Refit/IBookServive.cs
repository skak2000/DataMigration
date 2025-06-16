using DataMigration.Modules.Author;
using DataMigration.Modules.Chapter;
using DataMigration.Modules.Story;
using Refit;

namespace DataMigration.Refit
{
    public interface IBookService
    {
        [Post("/Authors/CreateAuthor")]
        Task<List<AuthorRespons>> CreateAuthors([Body] List<AuthorRequest> authors, Guid tenantId, Guid instanceId);

        [Post("/books/CreateBooks")]
        Task<List<StoryRespons>> CreateBooks([Body] List<StoryRequest> storys, Guid tenantId, Guid instanceId);

        [Post("/chapter/CreateChapters")]
        Task<List<ChapterRespons>> CreateChapters([Body] List<ChapterRequest> input, Guid tenantId, Guid instanceId);
    }
}
