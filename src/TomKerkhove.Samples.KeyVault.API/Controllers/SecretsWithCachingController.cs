using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using TomKerkhove.Samples.KeyVault.API.Contracts;
using TomKerkhove.Samples.KeyVault.API.Providers;

namespace TomKerkhove.Samples.KeyVault.API.Controllers
{
    [Route("api/v1/orders/", Name = "Scenario 3 - Cached Secrets")]
    public class SecretsWithCachingController : Controller
    {
        private const string OrdersQueueName = "orders";
        private const string SecretName = "ServiceBus";
        private readonly ISecretProvider secretProvider;

        public SecretsWithCachingController(ISecretProvider secretProvider)
        {
            this.secretProvider = secretProvider;
        }

        [HttpPost]
        [SwaggerOperation("Create Order")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] OrderContract order)
        {
            var orderMessage = CreateOrderMessage(order);

            var connectionString = await secretProvider.GetSecretAsync(SecretName);

            var sender = new MessageSender(connectionString, OrdersQueueName);
            await sender.SendAsync(orderMessage);

            return Ok(order);
        }

        private static Message CreateOrderMessage(OrderContract order)
        {
            var rawOrderInJson = JsonConvert.SerializeObject(order);
            var rawOrderInBytes = Encoding.UTF8.GetBytes(rawOrderInJson);
            return new Message(rawOrderInBytes);
        }
    }
}