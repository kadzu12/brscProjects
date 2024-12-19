using brskProject.Models;

namespace brskProject.AdditoanalClasses
{
    public class TokenResponse
    {
        public User User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

}
