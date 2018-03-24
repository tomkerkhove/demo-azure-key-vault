﻿using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using Polly;
using Swashbuckle.AspNetCore.SwaggerGen;
using TomKerkhove.Samples.KeyVault.API.Contracts;
using TomKerkhove.Samples.KeyVault.API.Providers.Interfaces;

namespace TomKerkhove.Samples.KeyVault.API.Controllers
{
    [Route("api/v1/orders/", Name = "Scenario 3 - Cached Secrets")]
    public class SecretsWithCachingController : Controller
    {
        private const string OrdersQueueName = "orders";
        private const string SecretName = "ServiceBus-ConnectionString";
        private readonly ICachedSecretProvider secretProvider;
        private readonly ITelemetryProvider telemetryProvider;

        public SecretsWithCachingController(ICachedSecretProvider secretProvider, ITelemetryProvider telemetryProvider)
        {
            this.secretProvider = secretProvider;
            this.telemetryProvider = telemetryProvider;
        }

        [HttpPost]
        [SwaggerOperation("Create Order")]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(OrderContract), "Order was succesfully created")]
        [SwaggerResponse((int) HttpStatusCode.InternalServerError, typeof(string), "We were unable to process your request")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] OrderContract order)
        {
            try
            {
                var rawOrder = CreateOrderMessage(order);

                await ProcessNewOrderAsync(rawOrder);

                return Ok(order);
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

        private static byte[] CreateOrderMessage(OrderContract order)
        {
            var rawOrderInJson = JsonConvert.SerializeObject(order);
            var rawOrderInBytes = Encoding.UTF8.GetBytes(rawOrderInJson);
            return rawOrderInBytes;
        }

        private static async Task QueueMessageAsync(byte[] rawOrder, string connectionString)
        {
            var orderMessage = new Message(rawOrder);
            var sender = new MessageSender(connectionString, OrdersQueueName);
            await sender.SendAsync(orderMessage);
        }

        private async Task ProcessNewOrderAsync(byte[] rawOrder)
        {
            var connectionString = await secretProvider.GetSecretAsync(SecretName);

            var retryPolicy = Policy.Handle<UnauthorizedAccessException>()
                .RetryAsync(retryCount: 5, onRetryAsync: async (exception, retryCount, context) => connectionString = await secretProvider.GetSecretAsync(SecretName, ignoreCache: true));

            await retryPolicy.ExecuteAsync(async () => await QueueMessageAsync(rawOrder, connectionString));
        }
    }
}