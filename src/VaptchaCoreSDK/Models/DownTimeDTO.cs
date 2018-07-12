namespace VaptchaCoreSDK
{
    public class DownTimeDTO
    {
        public ErrorDto Error { get; set; }
        public DownTimeDto DownTime { get; set; }
        public SignatureDto Signature { get; set; }
        public DownTimeCheckDto DownTimeCheck { get; set; }
        public DwonTimeState State { get; set; }
    }
}
