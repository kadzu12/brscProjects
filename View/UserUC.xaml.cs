using brskClient.DTO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using brskClient.ApiClass;

namespace brskClient.View
{
    /// <summary>
    /// Логика взаимодействия для UserUC.xaml
    /// </summary>
    public partial class UserUC : UserControl
    {
        UserDTO userContext;
        public UserUC(UserDTO userDTO)
        {
            InitializeComponent();
            DataContext = userDTO;
            userContext = userDTO;
            List<string> listRole = new List<string>() { "Админ", "Менеджер", "Пользователь" };
            RoleCB.ItemsSource = listRole;
            RoleCB.SelectedIndex = userContext.RoleId-1;
        }

        private async void EditBut_Click(object sender, RoutedEventArgs e)
        {
            if(Api.userSignIn.RoleId == 1)
            {
                if (userContext.Id == Api.userSignIn.Id)
                {
                    MessageBox.Show("Себя изменить нельзя.");
                    return;
                }
                userContext.Name = NameTb.Text;
                userContext.Email = EmailTb.Text;
                userContext.PasswordHash = PassTb.Text;
                userContext.RoleId = (int)RoleCB.SelectedIndex + 1;
                await Api.EditUser(userContext);
            }
            else
            {
                MessageBox.Show("Недостаточно прав.");
            }
        }

        private async void DeleteBut_Click(object sender, RoutedEventArgs e)
        {
            if (Api.userSignIn.RoleId == 1)
            {
                if(userContext.Id == Api.userSignIn.Id)
                {
                    MessageBox.Show("Себя удалить нельзя.");
                    return;
                }
                await Api.DeleteUser(userContext);
            }
            else
            {
                MessageBox.Show("Недостаточно прав.");
            }
        }
    }
}
