using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using StreamingAPI.Models;

namespace StreamingAPI.Services
{
    // --- Yardımcı Sınıflar (Veritabanı yapın için şart!) ---
    public class AdminInfo
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }

    public class DbRoot
    {
        public AdminInfo admin { get; set; } = new AdminInfo();
        public List<Content> contents { get; set; } = new List<Content>();
    }

    public class ContentService : IContentService
    {
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "Data", "db.json");
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        private DbRoot ReadDatabase()
        {
            if (!File.Exists(_filePath)) return new DbRoot();
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<DbRoot>(json, _options) ?? new DbRoot();
            }
            catch { return new DbRoot(); }
        }

        private void WriteDatabase(DbRoot db)
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize(db, _options));
        }

        public List<Content> GetAll()
        {
            var db = ReadDatabase();
            return db.contents ?? new List<Content>();
        }

        public Content Add(Content content)
        {
            var db = ReadDatabase();
            content.Id = Guid.NewGuid();
            content.CreatedAt = DateTime.UtcNow;

            if (db.contents == null) db.contents = new List<Content>();
            db.contents.Add(content);

            WriteDatabase(db);
            return content;
        }

        public Content? Update(Guid id, Content updated)
        {
            var db = ReadDatabase();
            if (db.contents == null) return null;

            var existing = db.contents.FirstOrDefault(x => x.Id == id);
            if (existing == null) return null;

            existing.Title = updated.Title;
            existing.Genre = updated.Genre;
            existing.Year = updated.Year;
            existing.Description = updated.Description;
            existing.Director = updated.Director;
            existing.Season = updated.Season;
            existing.Episodes = updated.Episodes;
            existing.VideoUrl = updated.VideoUrl;
            existing.ThumbnailUrl = updated.ThumbnailUrl;

            WriteDatabase(db);
            return existing;
        }

        public bool Delete(Guid id)
        {
            var db = ReadDatabase();
            if (db.contents == null) return false;

            var item = db.contents.FirstOrDefault(x => x.Id == id);
            if (item == null) return false;

            db.contents.Remove(item);
            WriteDatabase(db);
            return true;
        }

        public Content? IncrementViewCount(Guid id)
        {
            var db = ReadDatabase();
            if (db.contents == null) return null;

            var item = db.contents.FirstOrDefault(x => x.Id == id);
            if (item == null) return null;

            item.ViewCount++;
            WriteDatabase(db);
            return item;
        }
    }
}