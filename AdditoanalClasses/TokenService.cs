using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using brskProject.Models;
using System.IO;
using System.Timers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using brskProject.DTO;
using brskProject.Controllers;

namespace brskProject.AdditoanalClasses
{
    public class TokenService
    {
        private readonly string _key;

        public TokenService(string key)
        {
            _key = key;
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, GetRoleName((int)user.RoleId)) // Преобразуем RoleId в имя роли
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, GetRoleName((int)user.RoleId)) // Преобразуем RoleId в имя роли
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private string GetRoleName(int roleId)
        {
            switch (roleId)
            {
                case 1: return "Админ";
                case 2: return "Менеджер";
                case 3: return "Пользователь";
                default: throw new ArgumentException("Invalid role ID");
            }
        }
        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken.ValidTo > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        public void SaveTokensToFile(string accessToken, string refreshToken, string filePath)
        {
            var tokenInfo = $"AccessToken: {accessToken}\nGeneratedAt: {DateTime.UtcNow}\nRefreshToken: {refreshToken}\nGeneratedAt: {DateTime.UtcNow}";
            File.WriteAllText(filePath, tokenInfo);
        }

        public (string accessToken, string refreshToken) ReadTokensFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fileContent = File.ReadAllText(filePath);
                var tokenInfo = fileContent.Split(new[] { "\n" }, StringSplitOptions.None);
                var accessToken = tokenInfo[0].Replace("AccessToken: ", "");
                var refreshToken = tokenInfo[2].Replace("RefreshToken: ", "");
                return (accessToken, refreshToken);
            }
            return (null, null);
        }

        public void DeleteTokenFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void DeleteAllTokenFiles(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                var files = Directory.GetFiles(directoryPath, "*.txt");
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
        }

        public static List<int> GetUserIdsFromFiles()
        {
            var directoryPath = @"C:\Users\sprus\source\repos\brskClient\Assests\files\";
            if (Directory.Exists(directoryPath))
            {
                var files = Directory.GetFiles(directoryPath, "*.txt");
                var userIds = new List<int>();
                foreach (var file in files)
                {
                    var userId = int.Parse(Path.GetFileNameWithoutExtension(file).Replace("_token", ""));
                    userIds.Add(userId);
                }
                return userIds;
            }
            return new List<int>();
        }

        public static bool CheckUserInFiles(UserDTO userDTO)
        {
            var directoryPath = @"C:\Users\sprus\source\repos\brskClient\Assests\files\";
            if (Directory.Exists(directoryPath))
            {
                var files = Directory.GetFiles(directoryPath, "*.txt");
                foreach (var file in files)
                {
                    var userId = int.Parse(Path.GetFileNameWithoutExtension(file).Replace("_token", ""));
                    if (userId == userDTO.Id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task CheckAndRefreshToken()
        {
            if (UserController.userSignIn != null)
            {
                var directoryPath = @"C:\Users\sprus\source\repos\brskClient\Assests\files\";
                var filePath = Path.Combine(directoryPath, $"{UserController.userSignIn.Id}_token.txt");

                if (File.Exists(filePath))
                {
                    var tokenService = new TokenService("kadzuqwertyuiopasdfghjklzxcvbnmmnbvcxzasdfghjkl");
                    var (accessToken, refreshToken) = tokenService.ReadTokensFromFile(filePath);

                    if (refreshToken != null && !tokenService.ValidateToken(refreshToken))
                    {
                        if (accessToken != null && tokenService.ValidateToken(accessToken))
                        {
                            var user = await GetUserById(UserController.userSignIn.Id);
                            if (user != null)
                            {
                                var newAccessToken = tokenService.GenerateToken(user);
                                tokenService.SaveTokensToFile(newAccessToken, refreshToken, filePath);
                            }
                        }
                        else
                        {
                            tokenService.DeleteTokenFile(filePath);
                            UserController.userSignIn = null;
                        }
                    }
                }
            }
        }
        private static async Task<User> GetUserById(int userId)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://localhost:7121/api/User/getUserById/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var userJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<User>(userJson);
                }
            }
            return null;
        }
    }
}
