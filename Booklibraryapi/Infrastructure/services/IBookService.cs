using Domain.Entities;

namespace Infrastructure.Services
{
    public interface IBookService
    {
        Task<List<Book>> FetchBooksFromApiAsync(string query);
    }
}
