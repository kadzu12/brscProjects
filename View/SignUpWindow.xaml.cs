using brskClient.ApiClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using brskClient.DTO;
using System.IO;

namespace brskClient.View
{
    /// <summary>
    /// Логика взаимодействия для SignUpWindow.xaml
    /// </summary>
    public  partial class SignUpWindow : Window
    {
        public SignUpWindow()
        {
            InitializeComponent();
        }
        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
        }
        private bool IsValidPassword(string password)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(password, "^(?=.*[a-zA-Z])(?=.*[0-9])");
        }

        private bool IsValidString(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private void BackBut_Click(object sender, RoutedEventArgs e)
        {
            SignInWindow signInWindow = new SignInWindow();
            signInWindow.Show();
            this.Close();
        }

        private async void RegBut_Click(object sender, RoutedEventArgs e)
        {
            var name = NameTb.Text;
            var email = EmailTb.Text;
            var password = PassTb.Text;

            if (!IsValidString(name))
            {
                MessageBox.Show("Поле имени не должно быть пустым.");
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Неверная почта. Почта должна содержать только английские буквы и знак @.");
                return;
            }

            if (!IsValidPassword(password))
            {
                MessageBox.Show("Неверный пароль. Пароль должен содержать хотя бы одну букву и одну цифру.");
                return;
            }

            var userDto = new UserDTO
            {
                Name = name,
                Email = email,
                PasswordHash = password,
                RoleId = 1
            };

            try
            {
                var user = await Api.SignUp(userDto);

                if (user != null)
                {
                    MessageBox.Show("Регистрация успешна.");

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Регистрация не удалась: несоответствие данных запроса и ответа.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Регистрация не удалась: {ex.Message}");
            }
        }

        private void LogInbut_Click(object sender, RoutedEventArgs e)
        {
            SignInWindow signInWindow = new SignInWindow();
            signInWindow.Show();
            this.Close();
        }
    }
}
