using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using StreamingAPI.Models;
using StreamingAPI.Services;

namespace StreamingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _service;
        private readonly string tmdbApiKey = "308f79d27124cbfd796645b2c991d8a0"; 

        public ContentController(IContentService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] string search = "", [FromQuery] string type = "", [FromQuery] string genre = "", [FromQuery] string sortBy = "", [FromQuery] bool desc = true, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var list = _service.GetAll().AsEnumerable();

            if (!string.IsNullOrEmpty(search))
                list = list.Where(x => x.Title != null && x.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(type))
                list = list.Where(x => x.Type == type);

            if (!string.IsNullOrEmpty(genre))
                list = list.Where(x => x.Genre == genre);

            // EKSİK OLAN TITLE VE YEAR SIRALAMALARI EKLENDİ
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.Equals("viewcount", StringComparison.OrdinalIgnoreCase))
                    list = desc ? list.OrderByDescending(x => x.ViewCount) : list.OrderBy(x => x.ViewCount);
                else if (sortBy.Equals("likecount", StringComparison.OrdinalIgnoreCase))
                    list = desc ? list.OrderByDescending(x => x.LikeCount) : list.OrderBy(x => x.LikeCount);
                else if (sortBy.Equals("title", StringComparison.OrdinalIgnoreCase))
                    list = list.OrderBy(x => x.Title); // İsimleri A'dan Z'ye sıralar
                else if (sortBy.Equals("year", StringComparison.OrdinalIgnoreCase))
                    list = list.OrderByDescending(x => x.Year); // Yılları yeniden eskiye sıralar
                else
                    list = desc ? list.OrderByDescending(x => x.CreatedAt) : list.OrderBy(x => x.CreatedAt);
            }
            else
            {
                list = desc ? list.OrderByDescending(x => x.CreatedAt) : list.OrderBy(x => x.CreatedAt);
            }

            var totalCount = list.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new { items = items, totalPages = totalPages });
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var list = _service.GetAll();
            var stats = new
            {
                totalContent = list.Count,
                totalMovies = list.Count(x => x.Type == "movie"),
                totalSeries = list.Count(x => x.Type == "series"),
                genreDistribution = list.Where(x => !string.IsNullOrEmpty(x.Genre)).Select(x => x.Genre).Distinct().ToList()
            };
            return Ok(stats);
        }

        [HttpGet("recent")]
        public IActionResult GetRecent([FromQuery] string type = "", [FromQuery] int count = 5)
        {
            var list = FilterByType(type).OrderByDescending(x => x.CreatedAt).Take(count);
            return Ok(list);
        }

        [HttpGet("most-viewed")]
        public IActionResult GetMostViewed([FromQuery] string type = "", [FromQuery] int count = 5)
        {
            var list = FilterByType(type)
                .OrderByDescending(x => x.ViewCount)
                .ThenByDescending(x => x.CreatedAt)
                .Take(count);
            return Ok(list);
        }

        [HttpGet("most-liked")]
        public IActionResult GetMostLiked([FromQuery] string type = "", [FromQuery] int count = 5)
        {
            var list = FilterByType(type)
                .OrderByDescending(x => x.LikeCount)
                .ThenByDescending(x => x.CreatedAt)
                .Take(count);
            return Ok(list);
        }

        private IEnumerable<Content> FilterByType(string type)
        {
            var list = _service.GetAll().AsEnumerable();
            return string.IsNullOrEmpty(type)
                ? list
                : list.Where(x => x.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        [HttpGet("genres")]
        public IActionResult GetGenres()
        {
            var genres = _service.GetAll().Where(x => !string.IsNullOrEmpty(x.Genre)).Select(x => x.Genre).Distinct().ToList();
            return Ok(genres);
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id)
        {
            var list = _service.GetAll();
            var item = list.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("view/{id:guid}")]
        public IActionResult RegisterView(Guid id)
        {
            var item = _service.IncrementViewCount(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Content content)
        {
            if (string.IsNullOrEmpty(content.ThumbnailUrl) && !string.IsNullOrEmpty(content.Title))
            {
                content.ThumbnailUrl = await GetPosterUrlAsync(content.Title);
            }

            var created = _service.Add(content);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize]
        [HttpPut("{id:guid}")]
        public IActionResult Update(Guid id, [FromBody] Content content)
        {
            var updated = _service.Update(id, content);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [Authorize]
        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            var result = _service.Delete(id);
            if (!result) return NotFound();
            return NoContent();
        }

        private async Task<string> GetPosterUrlAsync(string title)
        {
            using var client = new HttpClient();
            var url = $"https://api.themoviedb.org/3/search/movie?api_key={tmdbApiKey}&query={Uri.EscapeDataString(title)}";

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var results = doc.RootElement.GetProperty("results");

                    if (results.GetArrayLength() > 0)
                    {
                        var posterPath = results[0].GetProperty("poster_path").GetString();
                        if (!string.IsNullOrEmpty(posterPath))
                        {
                            return $"https://image.tmdb.org/t/p/w500{posterPath}";
                        }
                    }
                }
            }
            catch
            {
            }

            return "";
        }
    }
}
