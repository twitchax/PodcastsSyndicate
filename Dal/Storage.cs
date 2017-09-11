using System;
using Microsoft.WindowsAzure.Storage;

namespace PodcastsSyndicate.Dal
{
    public static class Storage
    {
        public static CloudStorageAccount Self = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("StorageConn") ?? Helpers.Configuration["StorageConn"]);
    }
}