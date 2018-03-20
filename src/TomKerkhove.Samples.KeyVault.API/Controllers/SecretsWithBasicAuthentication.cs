using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TomKerkhove.Samples.KeyVault.API.Controllers
{
    [Route("api/v1/secrets/basic-auth/", Name = "Scenario 1 - Secrets with Basic Authentication")]
    public class SecretsWithBasicAuthentication : Controller
    {
        // You should never do this, but it's a demo so why bother!
        private readonly string adApplicationId = "666ef5f5-017d-4f01-b105-54fea4d9618f";
        private readonly string adApplicationSecret = "oKQTcEHlIZ7WKAiXqKt0DSC+i1HMOOueQnoHtXORpPs=";
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

        private async Task<string> AuthenticationCallback(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            var clientCredential = new ClientCredential(adApplicationId, adApplicationSecret);
            var token = await authContext.AcquireTokenAsync(resource, clientCredential).ConfigureAwait(continueOnCapturedContext: false);

            if (token == null)
            {
                throw new InvalidOperationException("Failed to obtain a token");
            }

            return token.AccessToken;
        }

        private KeyVaultClient GetKeyVaultClient()
        {
            return new KeyVaultClient(AuthenticationCallback);
        }
    }
}