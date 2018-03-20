﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;

namespace TomKerkhove.Samples.KeyVault.API.Builders
{
    public class KeyVaultAutenticationBuilder
    {
        private static readonly string adApplicationId = "666ef5f5-017d-4f01-b105-54fea4d9618f";
        private static readonly string adApplicationSecret = "oKQTcEHlIZ7WKAiXqKt0DSC+i1HMOOueQnoHtXORpPs=";

        private readonly KeyVaultClient.AuthenticationCallback authenticationCallback;

        private KeyVaultAutenticationBuilder(KeyVaultClient.AuthenticationCallback authenticationCallback)
        {
            this.authenticationCallback = authenticationCallback;
        }

        public static KeyVaultAutenticationBuilder UseBasicAuthentication()
        {
            return new KeyVaultAutenticationBuilder(BasicAuthenticationCallback);
        }

        public static KeyVaultAutenticationBuilder UseManagedServiceIdentity()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var authenticationCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
            return new KeyVaultAutenticationBuilder(authenticationCallback);
        }

        public KeyVaultClient Build()
        {
            if (authenticationCallback == null)
            {
                throw new Exception("No authentication was configured to use");
            }
            
            return new KeyVaultClient(authenticationCallback);
        }

        private static async Task<string> BasicAuthenticationCallback(string authority, string resource, string scope)
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
    }
}