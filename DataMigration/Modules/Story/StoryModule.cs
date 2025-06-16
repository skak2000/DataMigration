using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataMigration.Core;
using DataMigration.Core.DTO;
using DataMigration.Modules.Author;
using DataMigration.Refit;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;

namespace DataMigration.Modules.Story
{
    public class StoryModule : BasicModule
    {
        
        public StoryModule()
        {
            ModuleId = Guid.Parse("A5A72864-2445-4870-9C87-3AFF80A30F25");
            PriorityLevel = 2;
        }

        public override string Query(DataSyncCoreContext context)
        {
            // if error retry with half the numbers of rows.
            int select = SelectNumbers(100000);
                        
            var baseQuery = (from authors in context.StatusOnlineAuthors
                             join story in context.StatusOnlineStories
                             on authors.AuthorNameId equals story.AuthorNameId

                             // Remove allready transfere rows
                             join doneTable in context.DoneTables
                             on new { Key1 = story.AuthorNameId.ToString(), Key2 = story.StoryId.ToString() }
                             equals new { doneTable.Key1, doneTable.Key2 }
                             into doneGroup
                             from done in doneGroup.DefaultIfEmpty()

                             where done.TraceId == null && authors.IsDeleted == false
                             select new
                             {
                                 authors.AuthorName,
                                 authors.AuthorNameId,
                                 story.StoryId,
                                 story.StoryTitle,
                                 TraceId = authors.AuthorNameId.ToString() + "_" + story.StoryId.ToString()
                             });

            // TraceKey in new Database: AuthorNameId-StoryId
            string query = baseQuery.OrderBy(x => x.AuthorNameId).Take(select).ToQueryString();
            return query;
        }

        public override BasicDTO CreateDTO(DataTable data)
        {
            // Get the mappings from AuthorModule
            AuthorModule authorModule = new AuthorModule();
            List<CoreIdMap> mappings = GetMappingsByColumnName(data, "AuthorNameId", authorModule.ModuleId);

            StorySchema bookList = new StorySchema();
            List<CoreIdMap> missingValues = new List<CoreIdMap>();

            var mappingDict = mappings.ToDictionary(x => x.TraceId);

            foreach (DataRow row in data.Rows) 
            {
                // Get mapping and only transfere rows with mappings from highe level module
                //CoreIdMap? publicIdTemp = mappingDict.FirstOrDefault(x => x.TraceId == row["AuthorNameId"].ToString());
                mappingDict.TryGetValue(row["AuthorNameId"].ToString(), out CoreIdMap publicIdTemp);

                if (publicIdTemp != null)
                {
                    StoryRequest request = new StoryRequest()
                    {
                        AuthorId = Guid.Parse(publicIdTemp.Value),
                        Title = row.Field<string>("StoryTitle"),
                        Description = "Missing",
                        ISBN = "Missing",
                        TraceId = row["TraceId"].ToString()
                    };
                    bookList.DataList.Add(request);
                }
                else
                {
                    string traceId = row["TraceId"].ToString();

                    publicIdTemp = new CoreIdMap(traceId, Guid.Empty.ToString());
                    missingValues.Add(publicIdTemp);                    
                }
            }

            // Remove rows with out mappings
            RemoveRowFromDataset(missingValues);

            return bookList;
        }
        
        public async override Task<List<CoreIdMap>> SendData(BasicDTO input)
        {
            List<CoreIdMap> res = new List<CoreIdMap>();

            if (input is StorySchema schemaStory)
            {
                BookService bookService = new BookService();
                List<StoryRespons> respons = await bookService.bookApi.CreateBooks(schemaStory.DataList, TenantId, InstanceId);
                // Verify data is correct and return mapping for DoneTable
                res = VerifyData(schemaStory, respons);
            }
            return res;
        }

        /// <summary>
        /// Verify all data is correct
        /// </summary>
        /// <param name="model">Source</param>
        /// <param name="res">Webservices respons</param>
        /// <returns></returns>
        private List<CoreIdMap> VerifyData(StorySchema model, List<StoryRespons> res)
        {
            var mappingDict = model.DataList.ToDictionary(x => x.TraceId);
            List<CoreIdMap> mapping = new List<CoreIdMap>();

            foreach (StoryRespons item in res)
            {
                mappingDict.TryGetValue(item.TraceId, out StoryRequest? test);

                if (test != null && test.ISBN == item.ISBN && test.Title == item.Title && test.Description == item.Description)
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
