using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VaptchaCoreSDK
{
    public class VaptchaService : IVaptchaService
    {
        private readonly VaptchaOptions vaptchaOptions;
        private readonly VaptchaKeyOptions vaptchaKeyOptions;
        private readonly VaptchaHttpClient vaptchaHttpClient;
        private readonly IMemoryCache memoryCache;
        private readonly VaptchaDownCheckHttpClient vaptchaDownCheckHttpClient;
        private const string VaptchaServiceIsDownCacheKey = nameof(VaptchaService) + "_" + nameof(IsDown);
        private const string VaptchaServiceLastCheckDownTimeCacheKey = nameof(VaptchaService) + "_" + nameof(LastCheckDownTime);
        private const string VaptchaServicePassedSignaturesCacheKey = nameof(VaptchaService) + "_" + nameof(PassedSignatures);
        private const string VaptchaServicePublicKeyCacheKey = nameof(VaptchaService) + "_" + nameof(PublicKey);
        private bool IsDown
        {
            get => memoryCache.GetOrCreate(VaptchaServiceIsDownCacheKey, entry => { entry.SlidingExpiration = TimeSpan.MaxValue; return false; });
            set => memoryCache.Set(VaptchaServiceIsDownCacheKey, value);
        }

        private long LastCheckDownTime
        {
            get => memoryCache.GetOrCreate(VaptchaServiceLastCheckDownTimeCacheKey, entry => { entry.SlidingExpiration = TimeSpan.MaxValue; return 0; });
            set => memoryCache.Set(VaptchaServiceLastCheckDownTimeCacheKey, value);
        }

        private List<string> PassedSignatures
        {
            get => memoryCache.GetOrCreate(VaptchaServicePassedSignaturesCacheKey, entry => { entry.SlidingExpiration = TimeSpan.MaxValue; return new List<string>(); });
            set => memoryCache.Set(VaptchaServicePassedSignaturesCacheKey, value);
        }

        private string PublicKey => memoryCache.GetOrCreate(VaptchaServicePublicKeyCacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.MaxValue;
            return vaptchaDownCheckHttpClient.GetPublicKey().GetAwaiter().GetResult();
        });

        public VaptchaService(
            IOptions<VaptchaOptions> vaptchaOptions,
            IOptions<VaptchaKeyOptions> vaptchaKeyOptions,
            VaptchaHttpClient vaptchaHttpClient,
            IMemoryCache memoryCache,
            VaptchaDownCheckHttpClient vaptchaDownCheckHttpClient
            )
        {
            this.vaptchaOptions = vaptchaOptions?.Value ?? throw new ArgumentNullException(nameof(vaptchaOptions));
            this.vaptchaKeyOptions = vaptchaKeyOptions?.Value ?? throw new ArgumentNullException(nameof(vaptchaKeyOptions));
            this.vaptchaHttpClient = vaptchaHttpClient ?? throw new ArgumentNullException(nameof(vaptchaHttpClient));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.vaptchaDownCheckHttpClient = vaptchaDownCheckHttpClient ?? throw new ArgumentNullException(nameof(vaptchaDownCheckHttpClient));
        }

        public DownTimeDTO DownTime(string data) => DownTimeAsync(data).GetAwaiter().GetResult();

        public Task<DownTimeDTO> DownTimeAsync(string data)
        {
            throw new NotImplementedException();
        }

        public ChallengeDTO GetChallenge(string sceneId = "") => GetChallengeAsync(sceneId).GetAwaiter().GetResult();

        async public Task<ChallengeDTO> GetChallengeAsync(string sceneId = "")
        {
            var now = ToUnixTime(DateTime.Now);
            if (!IsDown)
            {
                var challenge = await vaptchaHttpClient.GetChallenge(now, sceneId);
                if (challenge == vaptchaOptions.RequestUsedUp)
                {
                    //进入宕机模式
                    //_lastCheckDownTime = now;
                    //_isDown = true;
                    PassedSignatures = new List<string>();
                    return new ChallengeDTO()
                    {
                        IsDownTime = true,
                        DownTime = GetDownTimeCaptcha()
                    };
                }
                if (string.IsNullOrEmpty(challenge))
                {
                    //判断宕机
                    if (await vaptchaDownCheckHttpClient.IsDown())
                    {
                        //进入宕机模式
                        LastCheckDownTime = now;
                        IsDown = true;
                        PassedSignatures = new List<string>();
                    }
                    return new ChallengeDTO()
                    {
                        IsDownTime = true,
                        DownTime = GetDownTimeCaptcha()
                    };
                }
                return new ChallengeDTO()
                {
                    IsDownTime = false,
                    Vaptcha = new ChallengeDto()
                    {
                        Id = vaptchaKeyOptions.VID,
                        Challenge = challenge
                    }
                };
            }
            else
            {
                if (now - LastCheckDownTime > vaptchaOptions.DownTimeCheckTime)
                {
                    LastCheckDownTime = now;
                    var challenge = await vaptchaHttpClient.GetChallenge(now, sceneId);
                    if (!string.IsNullOrEmpty(challenge) && challenge != vaptchaOptions.RequestUsedUp)
                    {
                        //退出宕机模式
                        IsDown = false;
                        PassedSignatures.Clear();
                        return new ChallengeDTO()
                        {
                            IsDownTime = false,
                            Vaptcha = new ChallengeDto()
                            {
                                Id = vaptchaKeyOptions.VID,
                                Challenge = challenge
                            }
                        };
                    }
                }
                return new ChallengeDTO()
                {
                    IsDownTime = true,
                    DownTime = GetDownTimeCaptcha()
                };
            }
        }

        private DownTimeDto GetDownTimeCaptcha()
        {
            long time = ToUnixTime(DateTime.Now);
            string md5 = Md5Encode(time + vaptchaKeyOptions.Key);
            string captcha = md5.Substring(0, 3);
            string verificationKey = md5.Substring(30);
            string url = Md5Encode(captcha + verificationKey + PublicKey) + vaptchaOptions.PicPostfix;
            url = vaptchaOptions.DownTimePath + url;
            return new DownTimeDto()
            {
                Time = time,
                Url = url
            };
        }

        private bool DownTimeValidate(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;
            string[] strs = token.Split(',');
            if (strs.Length < 2)
                return false;
            else
            {
                long time = Convert.ToInt64(strs[0]);
                string signature = strs[1];
                long now = ToUnixTime(DateTime.Now);
                if (now - time > vaptchaOptions.ValidatePassTime)
                    return false;
                else
                {
                    string signatureTrue = Md5Encode(time + vaptchaKeyOptions.Key + ConstString.vaptcha);
                    if (signatureTrue == signature)
                    {
                        if (PassedSignatures.Contains(signature))
                            return false;
                        else
                        {
                            PassedSignatures.Add(signature);
                            if (PassedSignatures.Count >= vaptchaOptions.MaxLength)
                            {
                                PassedSignatures.RemoveRange(0, PassedSignatures.Count - vaptchaOptions.MaxLength + 1);
                            }
                            return true;
                        }
                    }
                    else
                        return false;
                }
            }

        }

        public bool Validate(string challenge, string token, string sceneId = "") => ValidateAsync(challenge, token, sceneId).GetAwaiter().GetResult();

        async public Task<bool> ValidateAsync(string challenge, string token, string sceneId = "")
        {
            var now = ToUnixTime(DateTime.Now);
            if (!IsDown && !string.IsNullOrEmpty(challenge))
            {
                return await vaptchaHttpClient.Validate(challenge, token, now, sceneId);
            }
            else
            {
                return DownTimeValidate(token);
            }
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long ToUnixTime(DateTime date)
        {
            return Convert.ToInt64((date.ToUniversalTime() - Epoch).TotalMilliseconds);
        }

        private static string Md5Encode(string text)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string token = BitConverter.ToString(md5.ComputeHash(Encoding.GetEncoding("utf-8").GetBytes(text)));
            return token.Replace("-", "").ToLower();
        }
    }
}
