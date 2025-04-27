using Domain.Entities;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class BookService : IBookService
    {
        private readonly HttpClient _httpClient;

        public BookService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Book>> FetchBooksFromApiAsync(string query)
        {
            var url = $"https://openlibrary.org/search.json?q={query}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var openLibraryResponse = JsonSerializer.Deserialize<OpenLibraryResponse>(content);

            return openLibraryResponse?.Docs.Select(d => new Book
            {
                Title = d.Title,
                Author = d.AuthorName?.FirstOrDefault() ?? "Unknown",
                CoverImageUrl = d.CoverI != null ? $"https://covers.openlibrary.org/b/id/{d.CoverI}-M.jpg" : null
            }).ToList() ?? new List<Book>();
        }
    }

    public class OpenLibraryResponse
    {
        public List<Doc> Docs { get; set; }
    }

    public class Doc
    {
        public string Title { get; set; }
        public List<string> AuthorName { get; set; }
        public int? CoverI { get; set; }
    }
}
