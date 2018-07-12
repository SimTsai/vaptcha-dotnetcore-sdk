namespace VaptchCoreDemo.Models
{
    public class LoginInputModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Challenge { get; set; }
    }
}
