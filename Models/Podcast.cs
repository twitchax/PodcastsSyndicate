using System;
using System.Collections.Generic;
using DocumentDb.Fluent;

namespace PodcastsSyndicate.Models
{
    public class Podcast : HasId
    {
        // Id is unique title.
        public string Title { get; set; }
        //public string Link { get; set; }
        public string Language { get; set; }
        public string Copyright { get; set; }

        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string AuthorEmail { get; set; }
        public string Image { get; set; }
        public bool Explicit { get; set; } = false;

        public List<string> Categories { get; set; }

        public List<Episode> Episodes { get; set; }

        // Helpers methods.

        public string GetCacheKey()
        {
            return GetCacheKey(Id);
        }

        public static string GetCacheKey(string podcastId)
        {
            return $"rss_{podcastId}";
        }
    }
}