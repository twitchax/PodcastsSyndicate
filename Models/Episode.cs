using System;
using DocumentDb.Fluent;

namespace PodcastsSyndicate.Models
{
    public class Episode : HasId
    {
        public string Title { get; set; }
    }
}