using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TomKerkhove.Samples.KeyVault.API.Providers.Interfaces;

namespace TomKerkhove.Samples.KeyVault.API.Providers
{
    public class MemoryCachedSecretProvider : ICachedSecretProvider
    {
        private readonly MemoryCache memoryCache;
        private readonly ISecretProvider secretProvider;

        public MemoryCachedSecretProvider(ISecretProvider secretProvider)
        {
            this.secretProvider = secretProvider;

            memoryCache = CreateMemoryCache();
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            return await GetSecretAsync(secretName, ignoreCache: false);
        }

        public async Task<string> GetSecretAsync(string secretName, bool ignoreCache)
        {
            // Check if the secret is already cached, return it if it is
            if (ignoreCache == false && memoryCache.TryGetValue(secretName, out string secretValue))
            {
                return secretValue;
            }

            // Fetch latest secret from Key Vault
            var secret = await secretProvider.GetSecretAsync(secretName);

            // Store found secret in memory cache
            memoryCache.Set(secretName, secret, TimeSpan.FromHours(value: 1));

            return secret;
        }

        private static MemoryCache CreateMemoryCache()
        {
            var memoryCacheOptions = new MemoryCacheOptions();
            return new MemoryCache(memoryCacheOptions);
        }
    }
}