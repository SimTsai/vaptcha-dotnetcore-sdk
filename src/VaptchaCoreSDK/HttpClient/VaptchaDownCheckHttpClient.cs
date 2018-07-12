using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace VaptchaCoreSDK
{
    public class VaptchaDownCheckHttpClient
    {
        private readonly VaptchaOptions vaptchaOptions;
        private readonly HttpClient httpClient;

        public VaptchaDownCheckHttpClient(
            IOptions<VaptchaOptions> vaptchaOptions,
            HttpClient httpClient
            )
        {
            this.vaptchaOptions = vaptchaOptions?.Value ?? throw new ArgumentNullException(nameof(vaptchaOptions));
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            httpClient.BaseAddress = new Uri(this.vaptchaOptions.DownModeBasePath);
        }

        async public Task<bool> IsDown()
        {
            try
            {
                if (await httpClient.GetStringAsync(this.vaptchaOptions.IsDownPath) != ConstString.falseString)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        async public Task<string> GetPublicKey()
        {
            try
            {
                return await httpClient.GetStringAsync(this.vaptchaOptions.PublicKeyPath);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
