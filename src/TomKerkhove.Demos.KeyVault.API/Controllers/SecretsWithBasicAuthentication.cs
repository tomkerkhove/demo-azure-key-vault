using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Swashbuckle.AspNetCore.SwaggerGen;
using TomKerkhove.Demos.KeyVault.API.Providers.Interfaces;

namespace TomKerkhove.Demos.KeyVault.API.Controllers
{
    [Route("api/v1/secrets/basic-auth/", Name = "Scenario 1 - Secrets with Basic Authentication")]
    public class SecretsWithBasicAuthentication : Controller
    {
        // You should never do this, but it's a demo so why bother!
        private readonly string adApplicationId = "<application-id>";

        private readonly string adApplicationSecret = "<application-secret>";

        private readonly ITelemetryProvider telemetryProvider;
        private readonly string vaultUri = "https://secure-applications.vault.azure.net/";

        public SecretsWithBasicAuthentication(ITelemetryProvider telemetryProvider)
        {
            this.telemetryProvider = telemetryProvider;
        }

        [HttpGet("{secretName}")]
        [SwaggerOperation("Get Secret (Basic Authentication)")]
        public async Task<IActionResult> Get(string secretName)
        {
            try
            {
                var keyVaultClient = GetKeyVaultClient();
                var secret = await keyVaultClient.GetSecretAsync(vaultUri, secretName);

                return Ok(secret.Value);
            }
            catch (KeyVaultErrorException keyVaultException)
            {
                if (keyVaultException.Message.Contains("Secret not found:"))
                {
                    return NotFound("Secret not found");
                }

                throw;
            }
            catch (Exception exception)
            {
                telemetryProvider.LogException(exception);
                return StatusCode((int) HttpStatusCode.InternalServerError, "We were unable to process your request");
            }
        }

        [HttpPut("{secretName}")]
        [SwaggerOperation("Set Secret (Basic Authentication)")]
        public async Task<IActionResult> Put(string secretName, [FromBody] string secretValue)
        {
            try
            {
                var keyVaultClient = GetKeyVaultClient();
                await keyVaultClient.SetSecretAsync(vaultUri, secretName, secretValue);

                return NoContent();
            }
            catch (KeyVaultErrorException keyVaultException)
            {
                if (keyVaultException.Message.Contains("Secret not found:"))
                {
                    return NotFound("Secret not found");
                }

                throw;
            }
            catch (Exception exception)
            {
                telemetryProvider.LogException(exception);
                return StatusCode((int) HttpStatusCode.InternalServerError, "We were unable to process your request");
            }
        }

        private async Task<string> AuthenticationCallback(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            var clientCredential = new ClientCredential(adApplicationId, adApplicationSecret);
            var token = await authContext.AcquireTokenAsync(resource, clientCredential).ConfigureAwait(false);

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