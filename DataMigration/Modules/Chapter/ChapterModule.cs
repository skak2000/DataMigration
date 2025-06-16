using DataAccessLayer.Data;
using DataMigration.Core;
using DataMigration.Core.DTO;
using DataMigration.Modules.Author;
using DataMigration.Modules.Story;
using DataMigration.Refit;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DataMigration.Modules.Chapter
{
    public class ChapterModule : BasicModule
    {
        public ChapterModule()
        {
            ModuleId = Guid.Parse("A2338BC5-0A10-4B89-BB29-6F699E6364EA");
            PriorityLevel = 3;
        }

        public override string Query(DataSyncCoreContext context)
        {
            // if error retry with half the numbers of rows.
            int select = SelectNumbers(100000);

            var baseQuery = (from authors in context.StatusOnlineAuthors
                             join story in context.StatusOnlineStories.OrderBy(x => x.StoryId)
                             on authors.AuthorNameId equals story.AuthorNameId
                             join chapter in context.StatusOnlineChapters
                             on story.StoryId equals chapter.StoryId

                             // Remove allready transfere rows
                             join doneTable in context.DoneTables 
                             on (story.StoryId.ToString() + "_" + chapter.ChapterId.ToString())
                             equals doneTable.TraceId into doneGroup
                             from done in doneGroup.DefaultIfEmpty()
                             where done == null && authors.IsDeleted == false
                             select new
                             {
                                 authors.AuthorNameId,
                                 story.StoryId,
                                 chapter.ChapterId,
                                 chapter.ChapterTitle,
                                 chapter.ChapterText,
                                 chapter.ChapterUrl,
                                 StoryKey = story.AuthorNameId.ToString() + "_" + chapter.StoryId.ToString(),
                                 TraceId = story.StoryId.ToString() + "_" + chapter.ChapterId.ToString()
                             });

            // TraceKey in new Database: StoryId-ChapterId
            string query = baseQuery.OrderBy(x=>x.StoryId).Take(select).ToQueryString();
            return query;
        }

        public override BasicDTO CreateDTO(DataTable data)
        {
            ChapterSchema model = new ChapterSchema();
            List<CoreIdMap> missingValues = new List<CoreIdMap>();

            // Get mapping from Story module
            StoryModule module = new StoryModule();
            List<CoreIdMap> mappings = GetMappingsByColumnName(data, "StoryKey", module.ModuleId);
            var mappingDict = mappings.ToDictionary(x => x.TraceId);

            foreach (DataRow row in data.Rows)
            {
                string key = row["StoryKey"].ToString();                
                mappingDict.TryGetValue(key, out CoreIdMap publicIdTemp);

                if (publicIdTemp != null)
                {
                    ChapterRequest request = new ChapterRequest()
                    {
                        BookId = Guid.Parse(publicIdTemp.Value),
                        Title = row.Field<string>("ChapterTitle"),
                        Text = row.Field<string>("ChapterText"),
                        ChapterNumber = 0,
                        TraceId = row["TraceId"].ToString()
                    };
                    model.DataList.Add(request);
                }
                else
                {
                    string traceId = row["TraceId"].ToString();
                    publicIdTemp = new CoreIdMap(traceId, Guid.Empty.ToString());
                    missingValues.Add(publicIdTemp);
                }
            }

            // Remove rows with out mappings from pre module
            RemoveRowFromDataset(missingValues);

            return model;
        }

        public async override Task<List<CoreIdMap>> SendData(BasicDTO input)
        {
            List<CoreIdMap> res = new List<CoreIdMap>();

            if (input is ChapterSchema model)
            {
                BookService bookService = new BookService();

                List<ChapterRespons> respons = await bookService.bookApi.CreateChapters(model.DataList, TenantId, InstanceId);
                // Verify data is correct and return mapping for DoneTable
                res = VerifyData(model, respons);
            }
            return res;
        }

        /// <summary>
        /// Verify all data is correct
        /// </summary>
        /// <param name="model">Source</param>
        /// <param name="res">Webservices respons</param>
        /// <returns></returns>
        private List<CoreIdMap> VerifyData(ChapterSchema model, List<ChapterRespons> res)
        {
            List<CoreIdMap> mapping = new List<CoreIdMap>();
            Dictionary<string, ChapterRequest> mappingDict = model.DataList.ToDictionary(x => x.TraceId);

            foreach (ChapterRespons item in res)
            {
                mappingDict.TryGetValue(item.TraceId, out ChapterRequest? test);
               
                if (test != null && test.ChapterNumber == item.ChapterNumber && test.Title == item.Title)
                {
                    // Create mapping to DoneTable
                    CoreIdMap map = new CoreIdMap(item.TraceId, item.PublicId.ToString());
                    mapping.Add(map);
                }
                else
                {
                    // Throw data error ?
                }
            }
            return mapping;
        }
    }
}
