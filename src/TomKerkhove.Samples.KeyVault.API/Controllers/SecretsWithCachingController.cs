using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
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

        public SecretsWithCachingController(ICachedSecretProvider cachedSecretProvider)
        {
            secretProvider = cachedSecretProvider;
        }

        [HttpPost]
        [SwaggerOperation("Create Order")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OrderContract), "Order was succesfully created")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, typeof(string), "We were unable to process your request")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] OrderContract order)
        {
            try
            {
                var connectionString = await secretProvider.GetSecretAsync(SecretName);

                var orderMessage = CreateOrderMessage(order);

                var sender = new MessageSender(connectionString, OrdersQueueName);
                await sender.SendAsync(orderMessage);

                return Ok(order);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "We were unable to process your request");
            }
        }

        private static Message CreateOrderMessage(OrderContract order)
        {
            var rawOrderInJson = JsonConvert.SerializeObject(order);
            var rawOrderInBytes = Encoding.UTF8.GetBytes(rawOrderInJson);
            return new Message(rawOrderInBytes);
        }
    }
}