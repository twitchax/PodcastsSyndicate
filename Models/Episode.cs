using System;
using System.Collections.Generic;
using DocumentDb.Fluent;

namespace PodcastsSyndicate.Models
{
    public class Episode : HasId
    {
        public string Title { get; set; }
        public string Link { get; set; }

        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Image { get; set; }

        public DateTime PublishDate { get; set; } = DateTime.Now;
        public ulong Duration { get; set; }

        public List<string> Tags { get; set; }
    }
}