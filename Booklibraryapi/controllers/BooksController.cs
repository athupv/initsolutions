using Domain.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace BookLibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BookDbContext _dbContext;
        private readonly IBookService _bookService;

        public BooksController(BookDbContext dbContext, IBookService bookService)
        {
            _dbContext = dbContext;
            _bookService = bookService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            var books = await _bookService.FetchBooksFromApiAsync(query);
            return Ok(books);
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] List<Book> books)
        {
            await _dbContext.Books.AddRangeAsync(books);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Books saved successfully." });
        }

        [HttpGet]
        public IActionResult GetAll(int page = 1, int pageSize = 10, string? titleFilter = null)
        {
            var query = _dbContext.Books.AsQueryable();

            if (!string.IsNullOrEmpty(titleFilter))
                query = query.Where(b => b.Title.Contains(titleFilter));

            var totalCount = query.Count();

            var books = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new { totalCount, books });
        }

        [HttpPost("batch-upsert")]
        public async Task<IActionResult> BatchUpsert([FromBody] List<Book> books)
        {
            foreach (var book in books)
            {
                var existingBook = _dbContext.Books.FirstOrDefault(b => b.Title == book.Title);
                if (existingBook != null)
                {
                    existingBook.Author = book.Author;
                    existingBook.Description = book.Description;
                    existingBook.CoverImageUrl = book.CoverImageUrl;
                }
                else
                {
                    await _dbContext.Books.AddAsync(book);
                }
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Batch upsert completed." });
        }
    }
}
