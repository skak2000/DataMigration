using DataAccessLayer.Data;
using DataMigration.Core;
using DataMigration.Core.DTO;
using DataMigration.Refit;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DataMigration.Modules.Author
{
    public class AuthorModule : BasicModule
    {

        public AuthorModule()
        {
            ModuleId = Guid.Parse("E1C9259B-D568-4AA0-89C6-F67C3D05FA99");
            PriorityLevel = 1;
        }

        public override string Query(DataSyncCoreContext context)
        {
            // if error retry with half the numbers of rows.
            int select = SelectNumbers(100000);


            var baseQuery = (from authors in context.StatusOnlineAuthors

                                 // Remove allready transfere rows
                             join doneTable in context.DoneTables
                             //.Where(x => x.TenantId == TenantId && x.InstanceId == InstanceId)
                             on authors.AuthorNameId.ToString()
                             equals doneTable.Key1 into doneGroup
                             from done in doneGroup.DefaultIfEmpty()

                             where (done.TraceId == null) && authors.IsDeleted == false
                             select new
                             {
                                 authors.AuthorName,
                                 authors.AuthorNameId,
                                 TraceId = authors.AuthorNameId
                             });

            //var baseQuery = (from authors in context.StatusOnlineAuthors

            //                 // Remove allready transfere rows
            //                 join doneTable in context.DoneTables
            //                 .Where(x => x.TenantId == TenantId && x.InstanceId == InstanceId)
            //                 on authors.AuthorNameId.ToString()
            //                 equals doneTable.TraceId into doneGroup
            //                 from done in doneGroup.DefaultIfEmpty()

            //                 // ToDo Fileter on [Tenant] & [Instance]
            //                 where done == null && authors.IsDeleted == false
            //                 select new
            //                 {
            //                     authors.AuthorName,
            //                     authors.AuthorNameId,
            //                     TraceId = authors.AuthorNameId.ToString()
            //                 });

            // AuthorName cannot be changed in this system.
            // TraceKey in new Database: AuthorNameId

            string query = baseQuery.Take(select).ToQueryString();
            return query;
        }

        public override BasicDTO CreateDTO(DataTable data)
        {
            AuthorSchema res = new AuthorSchema();

            foreach (DataRow row in data.Rows)
            {
                AuthorRequest author = new AuthorRequest();
                author.AuthorName = row["AuthorName"].ToString();
                author.TraceId = row["AuthorNameId"].ToString();
                res.DataList.Add(author);
            }
            return res;
        }

        public async override Task<List<CoreIdMap>> SendData(BasicDTO input)
        {
            List<CoreIdMap> res = new List<CoreIdMap>();

            if (input is AuthorSchema schema)
            {
                BookService bookService = new BookService();
                List<AuthorRespons> respons = await bookService.bookApi.CreateAuthors(schema.DataList, TenantId, InstanceId);
                // Verify data is correct and return mapping for DoneTable
                res = VerifyData(schema, respons);
            }
            return res;
        }

        /// <summary>
        /// Verify all data is correct
        /// </summary>
        /// <param name="model">Source</param>
        /// <param name="res">Webservices respons</param>
        /// <returns></returns>
        private List<CoreIdMap> VerifyData(AuthorSchema model, List<AuthorRespons> res)
        {
            List<CoreIdMap> mapping = new List<CoreIdMap>();
            Dictionary<string, AuthorRequest> mappingDict = model.DataList.ToDictionary(x => x.TraceId);

            foreach (AuthorRespons item in res)
            {
                mappingDict.TryGetValue(item.TraceId, out AuthorRequest? test);               

                if (test != null && item.Name == test.AuthorName)
                {
                    // Create mapping to DoneTable
                    CoreIdMap map = new CoreIdMap(test.TraceId, item.PublicId.ToString());
                    mapping.Add(map);
                }
                else
                {
                    throw new Exception("Data do not match");
                }
            }
            return mapping;
        }
    }
}
