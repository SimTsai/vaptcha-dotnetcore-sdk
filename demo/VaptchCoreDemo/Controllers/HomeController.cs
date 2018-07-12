using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VaptchCoreDemo.Models;
using VaptchaCoreSDK;

namespace VaptchCoreDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IVaptchaService vaptchaService;

        public HomeController(IVaptchaService vaptchaService)
        {
            this.vaptchaService = vaptchaService ?? throw new ArgumentNullException(nameof(vaptchaService));
        }

        async public Task<object> GetRegisterChallenge()
        {
            var dto = await vaptchaService.GetChallengeAsync("02");
            if (dto.IsDownTime)
            {
                return dto.DownTime;
            }
            else
            {
                return dto.Vaptcha;
            }
        }

        async public Task<object> GetLoginChallenge()
        {
            var dto = await vaptchaService.GetChallengeAsync("01");
            if (dto.IsDownTime)
            {
                return dto.DownTime;
            }
            else
            {
                return dto.Vaptcha;
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Login(/*[FromForm]LoginInputModel model*/)
        {
            //vaptchaService.ValidateAsync()
            return View();
        }

        [HttpPost]
        async public Task<IActionResult> Login([FromForm]LoginInputModel model)
        {
            var captchValid = await vaptchaService.ValidateAsync(model.Challenge, model.Token, "01");
            if (!captchValid)
            {
                //ModelState.AddModelError("", "");
                return View();
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
