using System;
using System.Collections.Generic;
using DocumentDb.Fluent;

namespace PodcastsSyndicate.Models
{
    public class Podcast : HasId
    {
        public string Name { get; set; }

        IEnumerable<Episode> Episodes { get; set; }
    }
}