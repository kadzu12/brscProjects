using brskProject.AdditoanalClasses;
using brskProject.DTO;
using brskProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Identity.Data;

namespace brskProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public static UserDTO userSignIn;

        /// <summary>
        /// Получает информацию о пользователе по логину и паролю.
        /// </summary>
        /// <remarks>
        /// Этот метод позволяет получить информацию о пользователе, если он существует и пароль верный.
        /// Для использования этого метода, пользователь должен быть аутентифицирован.
        /// </remarks>
        /// <response code="200">Возвращает данные пользователя.</response>
        /// <response code="404">Если пользователь не найден или пароль неверен.</response>
        
        [HttpPost("signIn")]
        public async Task<ActionResult<TokenResponse>> Get([FromBody] LoginRequest loginRequest)
        {
            User? user = Program.context.Users.FirstOrDefault(u => u.Email == loginRequest.Login);
            if (user == null || !PasswordHasher.VerifyPassword(user.PasswordHash, loginRequest.Password))
            {
                return NotFound("Пользователь не найден.");
            }

            userSignIn = new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = loginRequest.Login,
                PasswordHash = loginRequest.Password,
                RoleId = (int)user.RoleId
            };

            var tokenService = new TokenService("kadzuqwertyuiopasdfghjklzxcvbnmmnbvcxzasdfghjkl");

            var filePath = Path.Combine(@"C:\Users\sprus\source\repos\brskClient\Assests\files\", $"{user.Id}_token.txt");
            var token = tokenService.GenerateToken(user);
            var refreshToken = tokenService.GenerateRefreshToken(user);
            tokenService.SaveTokensToFile(token, refreshToken, filePath);

            // Вызываем метод проверки и обновления токенов
            await tokenService.CheckAndRefreshToken();

            return Ok(user);
        }

        public class LoginRequest
        {
            public string Login { get; set; }
            public string Password { get; set; }
        }

        /// <summary>
        /// Получает информацию о пользователе по логину и паролю, а также токен.
        /// </summary>
        /// <param name="loginRequest">Объект с логином и паролем.</param>
        /// <remarks>
        /// Этот метод позволяет получить информацию о пользователе, если он существует и пароль верный.
        /// </remarks>
        /// <response code="200">Возвращает данные пользователя и токены.</response>
        /// <response code="404">Если пользователь не найден или пароль неверен.</response>
        [HttpPost("signInWithToken")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> SignInWithToken([FromBody] LoginRequest loginRequest)
        {
            User? user = Program.context.Users.FirstOrDefault(u => u.Email == loginRequest.Login);
            if (user == null || !PasswordHasher.VerifyPassword(user.PasswordHash, loginRequest.Password))
            {
                return NotFound("Пользователь не найден.");
            }

            userSignIn = new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = loginRequest.Login,
                PasswordHash = loginRequest.Password,
                RoleId = (int)user.RoleId
            };

            var tokenService = new TokenService("kadzuqwertyuiopasdfghjklzxcvbnmmnbvcxzasdfghjkl");

            var filePath = Path.Combine(@"C:\Users\sprus\source\repos\brskClient\Assests\files\", $"{user.Id}_token.txt");
            var token = tokenService.GenerateToken(user);
            var refreshToken = tokenService.GenerateRefreshToken(user);
            tokenService.SaveTokensToFile(token, refreshToken, filePath);

            // Вызываем метод проверки и обновления токенов
            await tokenService.CheckAndRefreshToken();

            return Ok(new TokenResponse { AccessToken = token, RefreshToken = refreshToken });
        }
        /// <summary>
        /// Получает информацию о пользователе по логину с использованием токена.
        /// </summary>
        /// <param name="login">Логин пользователя.</param>
        /// <remarks>
        /// Этот метод позволяет получить информацию о пользователе, если токен существует.
        /// Для использования этого метода, пользователь должен быть аутентифицирован.
        /// </remarks>
        /// <response code="200">Возвращает данные пользователя.</response>
        /// <response code="404">Если токен не найден или пользователь не найден.</response>
        [HttpGet("signInToken/{login}")]
        public async Task<ActionResult<User?>> SignInToken(string login)
        {
            List<int> listUsersId = TokenService.GetUserIdsFromFiles();
            if (listUsersId != null)
            {
                foreach (int userId in listUsersId)
                {
                    var user = Program.context.Users.FirstOrDefault(user => user.Id == userId && user.Email == login);
                    if (user != null)
                    {
                        var tokenService = new TokenService("kadzuqwertyuiopasdfghjklzxcvbnmmnbvcxzasdfghjkl");
                        await tokenService.CheckAndRefreshToken();
                        userSignIn = new UserDTO
                        {
                            Id = user.Id,
                            Name = user.Name,
                            Email = user.Email,
                            PasswordHash = user.PasswordHash,
                            RoleId = (int)user.RoleId
                        };
                        return Ok(user);
                    }
                }
            }
            return NotFound("Боюсь вашего токена нет в системе, попробуйте еще раз или авторизуйтесь.");
        }

        /// <summary>
        /// Выполняет выход пользователя из системы.
        /// </summary>
        /// <param name="userDto">Данные пользователя.</param>
        /// <remarks>
        /// Этот метод позволяет пользователю выйти из системы, удаляя его токены.
        /// Для использования этого метода, пользователь должен быть аутентифицирован.
        /// </remarks>
        /// <response code="200">Возвращает сообщение об успешном выходе.</response>
        [HttpPost("logout")]
        public ActionResult Logout([FromBody] UserDTO userDto)
        {
            var filePath = Path.Combine(@"C:\Users\sprus\source\repos\brskClient\Assests\files\", $"{userDto.Id}_token.txt");
            var refreshTokenFile = Path.Combine(@"C:\Users\sprus\source\repos\brskClient\Assests\files\", $"{userDto.Id}_refresh_token.txt");
            var tokenService = new TokenService("kadzuqwertyuiopasdfghjklzxcvbnmmnbvcxzasdfghjkl");
            tokenService.DeleteTokenFile(filePath);
            tokenService.DeleteTokenFile(refreshTokenFile);
            userSignIn = null;
            return Ok("Вы успешно вышли из аккаунта.");
        }

        /// <summary>
        /// Получает информацию о пользователе по логину.
        /// </summary>
        /// <param name="login">Логин пользователя.</param>
        /// <remarks>
        /// Этот метод позволяет получить информацию о пользователе, если он существует.
        /// Для использования этого метода, пользователь должен быть аутентифицирован.
        /// </remarks>
        /// <response code="200">Возвращает данные пользователя.</response>
        /// <response code="404">Если пользователь не найден.</response>
        [HttpGet("getUserByLogin/{login}")]
        [Authorize(Roles = "Админ, Менеджер")]
        public ActionResult<User?> GetUserByLogin(string login)
        {
            User? user = Program.context.Users.FirstOrDefault(user => user.Email == login);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }
            return Ok(user);
        }

        /// <summary>
        /// Получает информацию о пользователе по ID.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        /// <remarks>
        /// Этот метод позволяет получить информацию о пользователе, если он существует.
        /// Для использования этого метода, пользователь должен быть аутентифицирован.
        /// </remarks>
        /// <response code="200">Возвращает данные пользователя.</response>
        /// <response code="404">Если пользователь не найден.</response>
        [HttpGet("getUserById/{id}")]
        [Authorize(Roles = "Пользователь, Менеджер")]
        public ActionResult<User?> GetUserById(int id)
        {
            User? user = Program.context.Users.FirstOrDefault(user => user.Id == id);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }
            return Ok(user);
        }

        /// <summary>
        /// Получает информацию о пользователе по ID из файлов токенов.
        /// </summary>
        /// <remarks>
        /// Этот метод позволяет получить информацию о пользователе, если токен существует.
        /// Для использования этого метода, пользователь должен быть аутентифицирован.
        /// </remarks>
        /// <response code="200">Возвращает данные пользователя.</response>
        /// <response code="404">Если токен не найден или пользователь не найден.</response>
        [HttpGet("getUserByIdInTokenFiles")]
        public ActionResult<User?> GetUserByIdInTokenFiles()
        {
            List<int> listUsersId = TokenService.GetUserIdsFromFiles();
            if (listUsersId != null)
            {
                foreach (int userId in listUsersId)
                {
                    var user = Program.context.Users.FirstOrDefault(user => user.Id == userId);
                    if (user != null)
                    {
                        return Ok(user);
                    }
                }
            }
            return NotFound("У нас нет токена для продления вашей сессии, авторизуйтесь!");
        }

        /// <summary>
        /// Регистрирует нового пользователя.
        /// </summary>
        /// <param name="userDto">Данные нового пользователя.</param>
        /// <remarks>
        /// Этот метод позволяет зарегистрировать нового пользователя.
        /// Для использования этого метода, пользователь должен быть аутентифицирован.
        /// </remarks>
        /// <response code="201">Возвращает данные нового пользователя.</response>
        /// <response code="409">Если пользователь с таким логином уже существует.</response>
        [HttpPost("register")]
        public ActionResult<User> Register([FromBody] UserDTO userDto)
        {
            var existingUser = Program.context.Users.FirstOrDefault(u => u.Email == userDto.Email);
            if (existingUser != null) { return Conflict("Пользователь с таким логином уже существует."); }

            var newUser = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                PasswordHash = PasswordHasher.HashPassword(userDto.PasswordHash),
                RoleId = 3
            };

            Program.context.Users.Add(newUser);
            Program.context.SaveChanges();
            return StatusCode(201, newUser);
        }

        /// <summary>
        /// Получает токены для пользователя по логину и паролю.
        /// </summary>
        /// <param name="login">Логин пользователя.</param>
        /// <param name="password">Пароль пользователя.</param>
        /// <remarks>
        /// Этот метод позволяет получить токены для пользователя, если он существует и пароль верный.
        /// </remarks>
        /// <response code="200">Возвращает токены пользователя.</response>
        /// <response code="404">Если пользователь не найден или пароль неверен.</response>
        [HttpPost("get-token")]
        public ActionResult<TokenResponse> GetToken([FromBody] int id)
        {
            var directoryPath = @"C:\Users\sprus\source\repos\brskClient\Assests\files\";
            TokenResponse tokenResponse = null;
            if (Directory.Exists(directoryPath))
            {
                var files = Directory.GetFiles(directoryPath, "*.txt");

                foreach (var file in files)
                {
                    var userId = int.Parse(Path.GetFileNameWithoutExtension(file).Replace("_token", ""));
                    if (userId == id)
                    {
                        var tokenService = new TokenService("kadzuqwertyuiopasdfghjklzxcvbnmmnbvcxzasdfghjkl");
                        var readToken = tokenService.ReadTokensFromFile(file);
                        tokenResponse = new TokenResponse
                        {
                            AccessToken = readToken.accessToken,
                            RefreshToken = readToken.refreshToken
                        };
                        break;
                    }
                }
            }
            if (tokenResponse != null)
            {
                return Ok(tokenResponse);
            }
            return NotFound();
        }


        /// <summary>
        /// Получает список всех пользователей.
        /// </summary>
        /// <remarks>
        /// Этот метод позволяет получить список всех пользователей.
        /// Для использования этого метода, пользователь должен быть аутентифицирован.
        /// </remarks>
        /// <response code="200">Возвращает список пользователей.</response>
        [HttpGet("getUser")]
        [Authorize(Roles = "Админ,Менеджер")]
        public ActionResult<IEnumerable<UserDTO>> GetUser()
        {
            var users = Program.context.Users
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    PasswordHash = u.PasswordHash,
                    RoleId = (int)u.RoleId
                });

            return Ok(users);
        }

        /// <summary>
        /// Удаляет пользователя.
        /// </summary>
        /// <param name="userDTO">Данные пользователя.</param>
        /// <remarks>
        /// Этот метод позволяет удалить пользователя.
        /// Для использования этого метода, пользователь должен иметь роль "Admin".
        /// </remarks>
        /// <response code="200">Возвращает сообщение об успешном удалении.</response>
        /// <response code="404">Если пользователь не найден.</response>
        [HttpPut("deleteUser")]
        [Authorize(Roles = "Админ")]
        public ActionResult DeleteUser(UserDTO userDTO)
        {
            var user = Program.context.Users.FirstOrDefault(u => u.Id == userDTO.Id);
            if (user != null)
            {
                Program.context.Users.Remove(user);
                Program.context.SaveChanges();
                return Ok();
            }
            return NotFound("Пользователь не найден");
        }

        /// <summary>
        /// Редактирует информацию о пользователе.
        /// </summary>
        /// <param name="userDTO">Данные пользователя.</param>
        /// <remarks>
        /// Этот метод позволяет редактировать информацию о пользователе.
        /// Для использования этого метода, пользователь должен иметь роль "Admin".
        /// </remarks>
        /// <response code="200">Возвращает обновленные данные пользователя.</response>
        /// <response code="404">Если пользователь не найден.</response>
        /// <response code="409">Если логин уже используется другим пользователем.</response>
        [HttpPut("editUser")]
        [Authorize(Roles = "Пользователь, Админ")]
        public IActionResult EditUser(UserDTO userDTO)
        {
            var user = Program.context.Users.FirstOrDefault(p => p.Id == userDTO.Id);

            var existingUserWithSameLogin = Program.context.Users.FirstOrDefault(p => p.Id != userDTO.Id && p.Email == userDTO.Email);

            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            if (existingUserWithSameLogin != null)
            {
                return Conflict("Логин уже используется другим пользователем.");
            }

            user.Name = userDTO.Name;
            user.Email = userDTO.Email;
            user.PasswordHash = PasswordHasher.HashPassword(userDTO.PasswordHash);
            user.RoleId = userDTO.RoleId;

            Program.context.SaveChanges();

            return Ok(user);
        }
    }
}
