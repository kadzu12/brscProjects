using System;
using System.Collections.Generic;
using System.Linq;
using brskClient.ApiClass;
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
namespace brskClient.View
{
    /// <summary>
    /// Логика взаимодействия для SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window
    {
        public SignInWindow()
        {
            InitializeComponent();
            CheckFiles();
        }

        public async void CheckFiles()
        {
            var list = await Api.CheckUserToken();
            if(list != null)
            {
                OrTextB.Visibility= Visibility.Visible;
                SignInToken.Visibility= Visibility.Visible;
                MessageBox.Show("В системе есть токен, введите Email, чтобы проверить вы ли это.");
            }
        }

        private async void LogInBut_Click(object sender, RoutedEventArgs e)
        {
            var login = EmailTb.Text;
            var password = PassTb.Text;
            try
            {

                var user = await Api.SignIn(login, password);

                if (user != null)
                {
                    switch (user.RoleId)
                    {
                        case 3:
                            MainWindow vIewProduct = new MainWindow();
                            vIewProduct.Show();
                            this.Close();

                            break;
                        case 2:
                            AdminPanelWindow adminPanel = new AdminPanelWindow();
                            adminPanel.Show();
                            this.Close();
                            break;
                        case 1:
                            AdminPanelWindow adminPanel1 = new AdminPanelWindow();
                            adminPanel1.Show();
                            this.Close();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Неверный пароль или логин.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Авторизация не удалась: {ex.Message}");
            }
        }

        private void RegBut_Click(object sender, RoutedEventArgs e)
        {
            SignUpWindow signUpWindow = new SignUpWindow();
            signUpWindow.Show();
            this.Close();
        }

        private void BackBut_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();   
            window.Show();
            this.Close();
        }

        private async void SignInToken_Click(object sender, RoutedEventArgs e)
        {
            var user = await Api.SignInToken(EmailTb.Text);
            if (user != null)
            {
                switch (user.RoleId)
                {
                    case 3:
                        MainWindow vIewProduct = new MainWindow();
                        vIewProduct.Show();
                        this.Close();

                        break;
                    case 2:
                        AdminPanelWindow adminPanel = new AdminPanelWindow();
                        adminPanel.Show();
                        this.Close();
                        break;
                    case 1:
                        AdminPanelWindow adminPanel1 = new AdminPanelWindow();
                        adminPanel1.Show();
                        this.Close();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Боюсь вашего токена нет в системе, попробуйте еще раз или авторизуйтесь.");
            }
        }
    }
}
