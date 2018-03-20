using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using TomKerkhove.Samples.KeyVault.API.Builders;

namespace TomKerkhove.Samples.KeyVault.API.Providers
{
    public class SecretProvider : ISecretProvider
    {
        private readonly KeyVaultClient keyVaultClient;

        public SecretProvider()
        {
            var isDevelopment = true;

            var keyVaultAutenticationBuilder = isDevelopment ? KeyVaultAutenticationBuilder.UseBasicAuthentication() : KeyVaultAutenticationBuilder.UseManagedServiceIdentity();
            keyVaultClient = keyVaultAutenticationBuilder.Build();
        }

        // You should never do this, but it's a demo so why bother!
        public string VaultUri { get; } = "https://secure-applications.vault.azure.net/";

        public async Task<string> GetSecretAsync(string secretName)
        {
            var secret = await keyVaultClient.GetSecretAsync(VaultUri, secretName);

            return secret.Value;
        }
    }
}