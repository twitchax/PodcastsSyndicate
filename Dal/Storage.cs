using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PodcastsSyndicate.Dal
{
    public static class Storage
    {
        public static CloudStorageAccount Self = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("StorageConn") ?? Helpers.Configuration["StorageConn"]);
        public static CloudBlobContainer Container = Self.CreateCloudBlobClient().GetContainerReference("podcastssyndicate");

        public static async Task<CloudBlob> GetBlobDetails(string path)
        {
            var blob = Container.GetBlobReference(path);

            await blob.FetchAttributesAsync();

            return blob;
        }
    }
}