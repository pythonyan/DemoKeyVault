using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;

namespace DemoKeyVault.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public string Message { get; set; }


        /// <summary>
        /// Uso la libreria AppAuthentication 
        /// per accedere ai secrets del mio key vault.
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            Message = "App di esempio AZure Key Vault.";
            int retries = 0;
            bool retry = false;
            try
            {

                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient.GetSecretAsync(GetKeyVaultSecretsEndpoint())
                        .ConfigureAwait(false);
                Message = secret.Value;
            }
            
            /// <exception cref="KeyVaultErrorException">
            /// Sollevo un eccezione se l'operazione ritorni uno status code non valido
            /// </exception>
            catch (KeyVaultErrorException keyVaultException)
            {
                Message = keyVaultException.Message;
            }
        }


        /// <summary>
        /// Questo metodo implementa un algoritmo di exponential backoff 
        /// se riceviamo 429 errore da Azure Key Vault 
        /// </summary>
        /// <param name="retryCount">retryCount</param>
        /// <returns></returns>
        private static long GetWaitTime(int retryCount)
        {
            long waitTime = ((long)Math.Pow(2, retryCount) * 100L);
            return waitTime;
        }

        // This method fetches a token from Azure Active Directory, which can then be provided to Azure Key Vault to authenticate
        public async Task<string> GetAccessTokenAsync()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://vault.azure.net");
            return accessToken;
        }
        private string GetKeyVaultSecretsEndpoint() => "https://vault-aspnetcore-web.vault.azure.net/secrets/MySuperSecretPassword";
    }

}
