using System.Threading.Tasks;

namespace VaptchaCoreSDK
{
    public interface IVaptchaService
    {
        ChallengeDTO GetChallenge(string sceneId = "");
        Task<ChallengeDTO> GetChallengeAsync(string sceneId = "");

        bool Validate(string challenge, string token, string sceneId = "");
        Task<bool> ValidateAsync(string challenge, string token, string sceneId = "");

        DownTimeDTO DownTime(string data);
        Task<DownTimeDTO> DownTimeAsync(string data);
    }
}
