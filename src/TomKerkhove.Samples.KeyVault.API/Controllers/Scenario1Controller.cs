using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TomKerkhove.Samples.KeyVault.API.Controllers
{
    [Route("api/v1/secrets/basic-auth/", Name="Secrets (Basic Authentication)")]
    public class Scenario1Controller : Controller
    {
        [HttpGet("{secretName}")]
        [SwaggerOperation("Get Secret (Basic Authentication)")]
        public string Get(string secretName)
        {
            return "value";
        }

        [HttpPut("{secretName}")]
        [SwaggerOperation("Set Secret (Basic Authentication)")]
        public void Put(string secretName, [FromBody] string secretValue)
        {
        }
    }
}