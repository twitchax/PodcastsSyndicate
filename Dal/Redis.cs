using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using StackExchange.Redis;
namespace PodcastsSyndicate.Dal
{
    public static class Redis
    {
        public static IDatabase Self = ConnectionMultiplexer.Connect($"{Environment.GetEnvironmentVariable("RedisUri") ?? Helpers.Configuration["RedisUri"]},ssl=true,password={Environment.GetEnvironmentVariable("RedisKey") ?? Helpers.Configuration["RedisKey"]}").GetDatabase();
        
        public static Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None) => Self.StringSetAsync(key, value, expiry, when, flags);
        public static async Task<string> StringGetAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            var value = await Self.StringGetAsync(key, flags);
            return value.IsNullOrEmpty ? null : value.ToString();
        }

        public static async Task<bool> KeyExistsAsync(string key, CommandFlags flags = CommandFlags.None) => await Self.KeyExistsAsync(key, flags);

        public static async Task<bool> KeyDeleteAsync(string key, CommandFlags flags = CommandFlags.None) => await Self.KeyDeleteAsync(key, flags);

        public static async Task<string> GetOrAddAsync(string key, Func<Task<string>> functor, TimeSpan? expiry = null)
        {
            var value = await StringGetAsync(key);

            if(value != null)
                return value;

            value = await functor();

            await StringSetAsync(key, value, expiry);

            return value;
        }
    }
}