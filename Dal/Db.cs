using DocumentDb.Fluent;
using Microsoft.Extensions.Configuration;
using PodcastsSyndicate.Models;

namespace PodcastsSyndicate.Dal
{
    public static class Db
    {
        public static IDatabase Self = 
            Account
                .Connect(Helpers.Configuration["DbUri"], Helpers.Configuration["DbKey"])
                .Database("PodcastsSyndicate");

        public static IDocumentCollection<User> Users = Self.Collection<User>();
        public static IDocumentCollection<Podcast> Podcasts = Self.Collection<Podcast>();
    }
}