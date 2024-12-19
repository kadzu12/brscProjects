using brskClient.ApiClass;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using brskClient.View;

namespace brskClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UserBut_Click(object sender, RoutedEventArgs e)
        {
            if(Api.userSignIn == null)
            {
                SignInWindow signInWindow = new SignInWindow();
                signInWindow.Show();
                this.Close();
            }
            else if(Api.userSignIn.RoleId == 1 || Api.userSignIn.RoleId == 2)
            {
                AdminPanelWindow adminPanelWindow = new AdminPanelWindow();
                adminPanelWindow.Show();
                this.Close();
            }
            else if(Api.userSignIn.RoleId == 3)
            {
                EditProfileWindow editProfileWindow = new EditProfileWindow();  
                editProfileWindow.Show();
                this.Close();
            }
        }

        private async void ExitBut_Click(object sender, RoutedEventArgs e)
        {
            if (Api.userSignIn != null)
            {
                var result = await Api.Logout();
                if (result.IsSuccessStatusCode)
                {
                    MessageBox.Show("Вы успешно вышли из аккаунта.");
                }
                else
                {
                    MessageBox.Show("Ошибка при выходе из аккаунта: " + result.ReasonPhrase);
                }
            }
        }
    }
}