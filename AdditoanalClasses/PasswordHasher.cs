using System.Security.Cryptography;
using System.Text;

namespace brskProject.AdditoanalClasses
{
    using BCrypt.Net;
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            return BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string hashedPassword, string inputPassword)
        {
            return BCrypt.Verify(inputPassword, hashedPassword);
        }
    }

}
