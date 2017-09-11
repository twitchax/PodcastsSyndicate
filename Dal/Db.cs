using System;
using DocumentDb.Fluent;
using Microsoft.Extensions.Configuration;
using PodcastsSyndicate.Models;

namespace PodcastsSyndicate.Dal
{
    public static class Db
    {
        public static IDatabase Self = 
            Account
                .Connect(Environment.GetEnvironmentVariable("DbUri") ?? Helpers.Configuration["DbUri"], Environment.GetEnvironmentVariable("DbKey") ?? Helpers.Configuration["DbKey"])
                .Database("PodcastsSyndicate");

        public static IDocumentCollection<User> Users = Self.Collection<User>();
        public static IDocumentCollection<Podcast> Podcasts = Self.Collection<Podcast>();
    }
}