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

namespace brskClient.View
{
    /// <summary>
    /// Логика взаимодействия для EditProfileWindow.xaml
    /// </summary>
    public partial class EditProfileWindow : Window
    {
        public EditProfileWindow()
        {
            InitializeComponent();
        }

        private async void SaveBut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UserDTO userDTO = Api.userSignIn;
                userDTO.Name = NameTb.Text;
                userDTO.Email = EmailTb.Text;
                userDTO.PasswordHash = PassTb.Text;

                var result = await Api.EditUser(userDTO);
                if (result.IsSuccessStatusCode)
                {
                    MessageBox.Show("Пользователь успешно обновлен");
                    MainWindow vIewProduct = new MainWindow();
                    vIewProduct.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении пользователя или юзер с таким логином уже есть");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void toMainBut_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void BackBut_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            this.Close();
        }
    }
}
