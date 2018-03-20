using System.Threading.Tasks;

namespace TomKerkhove.Samples.KeyVault.API.Providers
{
    public interface ISecretProvider
    {
        /// <summary>
        ///     Gets the value for a specific secret
        /// </summary>
        /// <param name="secretName">Name of the secret to use</param>
        Task<string> GetSecretAsync(string secretName);
    }
}