using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TomKerkhove.Samples.KeyVault.API.Controllers
{
    [Route("api/v1/secrets/managed-service-identity/", Name = "Secrets (Managed Service Identity)")]
    public class Scenario2Controller : Controller
    {
        private readonly string vaultUri = "https://secure-applications.vault.azure.net/";

        [HttpGet("{secretName}")]
        [SwaggerOperation("Get Secret (Basic Authentication)")]
        public async Task<string> Get(string secretName)
        {
            var keyVaultClient = GetKeyVaultClient();
            var secret = await keyVaultClient.GetSecretAsync(vaultUri, secretName);

            return secret.Value;
        }

        [HttpPut("{secretName}")]
        [SwaggerOperation("Set Secret (Basic Authentication)")]
        public async Task Put(string secretName, [FromBody] string secretValue)
        {
            var keyVaultClient = GetKeyVaultClient();
            await keyVaultClient.SetSecretAsync(vaultUri, secretName, secretValue);
        }

        private static KeyVaultClient GetKeyVaultClient()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            return keyVaultClient;
        }
    }
}