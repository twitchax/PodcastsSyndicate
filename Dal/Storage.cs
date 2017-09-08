using Microsoft.WindowsAzure.Storage;

namespace PodcastsSyndicate.Dal
{
    public static class Storage
    {
        public static CloudStorageAccount Self = CloudStorageAccount.Parse(Helpers.Configuration["StorageConnectionString"]);
    }
}