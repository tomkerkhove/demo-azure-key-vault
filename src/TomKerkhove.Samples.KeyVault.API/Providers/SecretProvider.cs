using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using TomKerkhove.Samples.KeyVault.API.Builders;

namespace TomKerkhove.Samples.KeyVault.API.Providers
{
    public class SecretProvider : ISecretProvider
    {
        // You should never do this, but it's a demo so why bother!
        private readonly string vaultUri = "https://secure-applications.vault.azure.net/";

        public async Task<string> GetSecretAsync(string secretName)
        {
            var isDevelopment = true;

            var keyVaultAutenticationBuilder = isDevelopment ? KeyVaultAutenticationBuilder.UseBasicAuthentication() : KeyVaultAutenticationBuilder.UseManagedServiceIdentity();

            var keyVaultClient = keyVaultAutenticationBuilder.Build();
            var secret = await keyVaultClient.GetSecretAsync(vaultUri, secretName);

            return secret.Value;
        }
    }
}