using System.Windows;

namespace DiffuserShop.Client
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtLogin.Text == "admin" && txtPassword.Password == "admin123")
            {
                
                MainWindow mainWindow = new MainWindow(txtLogin.Text);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Password = "";
                txtLogin.Focus();
            }
        }
    }
}