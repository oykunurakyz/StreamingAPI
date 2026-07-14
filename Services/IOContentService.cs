using System;
using System.Collections.Generic;
using StreamingAPI.Models;

namespace StreamingAPI.Services
{
    public interface IContentService
    {
        List<Content> GetAll();
        Content Add(Content content);
        Content? Update(Guid id, Content updated);
        bool Delete(Guid id);
        Content? IncrementViewCount(Guid id);
    }
}