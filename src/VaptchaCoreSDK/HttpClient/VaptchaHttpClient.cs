using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VaptchaCoreSDK
{
    public class VaptchaHttpClient
    {
        private readonly VaptchaOptions vaptchaOptions;
        private readonly VaptchaKeyOptions vaptchaKeyOptions;
        private readonly HttpClient httpClient;

        public VaptchaHttpClient(
            IOptions<VaptchaOptions> vaptchaOptions,
            IOptions<VaptchaKeyOptions> vaptchaKeyOptions,
            HttpClient httpClient,
            VaptchaDownCheckHttpClient vaptchaDownCheckHttpClient
            )
        {
            this.vaptchaOptions = vaptchaOptions?.Value ?? throw new ArgumentNullException(nameof(vaptchaOptions));
            this.vaptchaKeyOptions = vaptchaKeyOptions?.Value ?? throw new ArgumentNullException(nameof(vaptchaKeyOptions));
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            VaptchaDownCheckHttpClient = vaptchaDownCheckHttpClient ?? throw new ArgumentNullException(nameof(vaptchaDownCheckHttpClient));
            httpClient.BaseAddress = new Uri(this.vaptchaOptions.ApiUrl);
        }

        public VaptchaDownCheckHttpClient VaptchaDownCheckHttpClient { get; }

        async public Task<string> GetChallenge(long now, string sceneId = "")
        {
            var query = $"id={vaptchaKeyOptions.VID}&scene={sceneId}&time={now}&version={vaptchaOptions.Version}&sdklang={vaptchaOptions.SdkLang}";
            var signature = HMACSHA1(vaptchaKeyOptions.Key, query);
            query = $"{query}&signature={signature}";
            var uriBuilder = new UriBuilder(new Uri(httpClient.BaseAddress, vaptchaOptions.GetChallengeUrl))
            {
                Query = query
            };
            try
            {
                return await httpClient.GetStringAsync(uriBuilder.Uri);
            }
            catch
            {
                return string.Empty;
            }
        }

        async public Task<bool> Validate(string challenge, string token, long now, string sceneId = "")
        {
            if (string.IsNullOrWhiteSpace(challenge) || string.IsNullOrWhiteSpace(token) || token != Md5Encode(vaptchaKeyOptions.Key + ConstString.vaptcha + challenge))
            {
                return false;
            }
            var query = $"id={vaptchaKeyOptions.VID}&scene={sceneId}&token={token}&time={now}&version={vaptchaOptions.Version}&sdklang={vaptchaOptions.SdkLang}";
            var signature = HMACSHA1(vaptchaKeyOptions.Key, query);
            try
            {
                var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>{
                    new KeyValuePair<string, string>("id", vaptchaKeyOptions.VID),
                    new KeyValuePair<string, string>("scene", sceneId),
                    new KeyValuePair<string, string>("token", token),
                    new KeyValuePair<string, string>("time", now.ToString()),
                    new KeyValuePair<string, string>("version", vaptchaOptions.Version),
                    new KeyValuePair<string, string>("sdklang", vaptchaOptions.SdkLang),
                    new KeyValuePair<string, string>("signature", signature)
                });

                var responseMessage = await httpClient.PostAsync(vaptchaOptions.ValidateUrl, content);
                responseMessage.EnsureSuccessStatusCode();
                return await responseMessage.Content.ReadAsStringAsync() == ConstString.success;
            }
            catch
            {
                return false;
            }
        }

        private string HMACSHA1(string key, string text)
        {
            using (HMACSHA1 hmac = new HMACSHA1(Encoding.GetEncoding("utf-8").GetBytes(key)))
            {
                byte[] hashValue = hmac.ComputeHash(Encoding.GetEncoding("utf-8").GetBytes(text));

                return Convert.ToBase64String(hashValue).Replace("/", "").Replace("+", "").Replace("=", "");
            }
        }

        private static string Md5Encode(string text)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string token = BitConverter.ToString(md5.ComputeHash(Encoding.GetEncoding("utf-8").GetBytes(text)));
            return token.Replace("-", "").ToLower();
        }
    }
}
