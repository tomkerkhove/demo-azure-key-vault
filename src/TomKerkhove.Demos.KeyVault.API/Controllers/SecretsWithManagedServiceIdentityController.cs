using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Swashbuckle.AspNetCore.SwaggerGen;
using TomKerkhove.Demos.KeyVault.API.Providers.Interfaces;

namespace TomKerkhove.Demos.KeyVault.API.Controllers
{
    [Route("api/v1/secrets/managed-service-identity/", Name = "Scenario 2 - Secrets with Managed Service Identity")]
    public class SecretsWithManagedServiceIdentityController : Controller
    {
        private readonly ITelemetryProvider telemetryProvider;
        private readonly string vaultUri = "https://secure-applications.vault.azure.net/";

        public SecretsWithManagedServiceIdentityController(ITelemetryProvider telemetryProvider)
        {
            this.telemetryProvider = telemetryProvider;
        }

        [HttpGet("{secretName}")]
        [SwaggerOperation("Get Secret (Managed Service Identity)")]
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
        [SwaggerOperation("Set Secret (Managed Service Identity)")]
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

        private static KeyVaultClient GetKeyVaultClient()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient =
                new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            return keyVaultClient;
        }
    }
}