using brskClient.DTO;
using brskProject.Models;
using Microsoft.AspNetCore.Identity.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Timers;
using System.Windows;

namespace brskClient.ApiClass
{
    public static class Api
    {
        public static UserDTO userSignIn;
        public class TokenResponse
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }
        public class LoginRequest
        {
            public string Login { get; set; }
            public string Password { get; set; }
        }
        public static async Task<User?> SignIn(string login, string password)
        {
            using (var client = new HttpClient())
            {
                var loginRequest = new LoginRequest { Login = login, Password = password };
                var json = JsonConvert.SerializeObject(loginRequest);
                var contentLogin = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await client.PostAsync($"https://localhost:7121/api/User/signIn", contentLogin);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    User user = JsonConvert.DeserializeObject<User>(content);
                    var response = JsonConvert.DeserializeObject<dynamic>(content);
                    var token = response.Token;
                    userSignIn = new UserDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = login,
                        PasswordHash = password,
                        RoleId = (int)user.RoleId
                    };
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    //StartTokenRefreshTimer();
                    return user;
                }
                return null;
            }
        }

        public static async Task<TokenResponse> GetToken(int userId)
        {
            using (var client = new HttpClient())
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json");

                try
                {
                    var result = await client.PostAsync("https://localhost:7121/api/User/get-token", jsonContent);
                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync();
                        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);
                        return tokenResponse;
                    }
                    else
                    {
                        throw new Exception($"Failed to retrieve token. Status code: {result.StatusCode}");
                    }
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show($"Request error: {e.Message}");
                    throw;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Unexpected error: {e.Message}");
                    throw;
                }
            }
        }


        public static async Task<User?> CheckUserToken()
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync("https://localhost:7121/api/User/getUserByIdInTokenFiles");

                if (result.IsSuccessStatusCode)
                {
                    var userJson = await result.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<User>(userJson);
                    return user;
                }
                else
                {
                    return null;
                }
            }
        }

        public static async Task<User?> SignInToken(string login)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync($"https://localhost:7121/api/User/signInToken/{login}");
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    User user = JsonConvert.DeserializeObject<User>(content);
                    var response = JsonConvert.DeserializeObject<dynamic>(content);
                    userSignIn = new UserDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        PasswordHash = user.PasswordHash,
                        RoleId = (int)user.RoleId
                    };
                    return user;
                }
                return null;
            }
        }

        public static async Task<HttpResponseMessage> SignUp(UserDTO userDto)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(userDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await client.PostAsync("https://localhost:7121/api/User/register", content);
                return result;
            }
        }
        public static async Task<HttpResponseMessage> DeleteUser(UserDTO userDTO)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(userDTO);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var tokenResponse = await GetToken(userSignIn.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                var result = await client.PutAsync("https://localhost:7121/api/User/deleteUser", content);
                return result;
            }
        }
        public static async Task<IEnumerable<UserDTO>> GetUser()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var tokenResponse = await GetToken(userSignIn.Id);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

                    var result = await client.GetAsync("https://localhost:7121/api/User/getUser");
                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync();
                        var users = JsonConvert.DeserializeObject<IEnumerable<UserDTO>>(json);
                        return users;
                    }
                    else
                    {
                        throw new Exception($"Failed to retrieve users. Status code: {result.StatusCode}");
                    }
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show($"Request error: {e.Message}");
                    throw;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Unexpected error: {e.Message}");
                    throw;
                }
            }
        }

        public static async Task<HttpResponseMessage> EditUser(UserDTO userDTO)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(userDTO);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var tokenResponse = await GetToken(userSignIn.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                var result = await client.PutAsync("https://localhost:7121/api/User/editUser", content);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var contentNew = await result.Content.ReadAsStringAsync();
                    User user = JsonConvert.DeserializeObject<User>(contentNew);
                    if (userSignIn.RoleId == 3) {
                        userSignIn = new UserDTO
                        {
                            Id = user.Id,
                            Name = user.Name,
                            Email = user.Email,
                            PasswordHash = user.PasswordHash,
                        };
                    }

                }
                return result;
            }
        }
        public static async Task<HttpResponseMessage> Logout()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var json = JsonConvert.SerializeObject(userSignIn);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var tokenResponse = await GetToken(userSignIn.Id);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                    var result = await client.PostAsync("https://localhost:7121/api/User/logout", content);
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        userSignIn = null;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при выходе из системы: " + ex.Message);
                    return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                }
            }
        }
    }
}
