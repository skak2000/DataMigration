using Refit;

namespace DataMigration.Refit
{
    public class BookService
    {
        public readonly IBookService bookApi;

        public BookService()
        {
            bookApi = RestService.For<IBookService>("https://localhost:32769/api");
        }
    }
}
