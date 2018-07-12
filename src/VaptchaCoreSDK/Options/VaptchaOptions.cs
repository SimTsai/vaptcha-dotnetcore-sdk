namespace VaptchaCoreSDK
{
    public class VaptchaOptions
    {
        /// <summary>
        /// SDK版本号
        /// </summary>
        public string Version { get; set; } = "1.0.0";
        /// <summary>
        /// SDK语言
        /// </summary>
        public string SdkLang { get; set; } = "csharp";
        /// <summary>
        /// VaptchaAPI Url
        /// </summary>
        public string ApiUrl { get; set; } = "http://api.vaptcha.com";
        /// <summary>
        /// 获取流水号 Url
        /// </summary>
        public string GetChallengeUrl { get; set; } = "/challenge";
        /// <summary>
        /// 验证 Url
        /// </summary>
        public string ValidateUrl { get; set; } = "/validate";
        /// <summary>
        /// 验证数量使用完
        /// </summary>
        public string RequestUsedUp { get; set; } = "0209";
        /// <summary>
        /// 宕机模式检验恢复时间185000ms
        /// </summary>
        public long DownTimeCheckTime { get; set; } = 185000;
        /// <summary>
        /// 宕机模式二次验证失效时间十分钟
        /// </summary>
        public long ValidatePassTime { get; set; } = 600000;
        /// <summary>
        /// 宕机模式请求失效的时间25秒
        /// </summary>
        public long RequestAbateTime { get; set; } = 250000;
        /// <summary>
        /// 宕机模式验证等待时间2秒
        /// </summary>
        public long ValidateWaitTime { get; set; } = 2000;
        /// <summary>
        /// 宕机模式保存通过数量最大值50000，默认采用将宕机模式通过得到的标识存入_passedSignatures集合中，以此来保证二次验证只能使用一次
        /// 用户可以在后台进行自己的相关处理
        /// </summary>
        public int MaxLength { get; set; } = 50000;
        /// <summary>
        /// 验证图的后缀png
        /// </summary>
        public string PicPostfix { get; set; } = ".png";
        /// <summary>
        /// 宕机模式baseurl
        /// </summary>
        public string DownModeBasePath { get; set; } = "http://down.vaptcha.com";
        /// <summary>
        /// 宕机模式key路径
        /// </summary>
        public string PublicKeyPath { get; set; } = "/publickey";
        /// <summary>
        /// 是否宕机路径
        /// </summary>
        public string IsDownPath { get; set; } = "/isdown";
        /// <summary>
        /// 宕机模式图片路径
        /// </summary>
        public string DownTimePath { get; set; } = "downtime/";
    }
}
