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

namespace IndustrialMachineSimulator.UI
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public string? SelectedRole { get; private set; }
        public bool IslogoutRequested { get; private set; } = false;
        public string CurrentRole { get; }
        public LoginWindow(string currentRole)
        {
            InitializeComponent();
            CurrentRole = currentRole;
            txtPassword.Focus();
            if(CurrentRole =="Operator")
            {
                LogoutButton.IsEnabled = false;
                LogoutButton.Opacity = 0.4;
            
            }
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string password=txtPassword.Password.Trim();
            if(password=="1")
            {
                SelectedRole = "Engineer";
                DialogResult = true;
                Close();
            }
            else if (password == "2")
            {
                SelectedRole = "Developer";
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Wrong password.", "Login failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void LogoutButton_Click(Object sender, RoutedEventArgs e)
        {
            IslogoutRequested = true;
            DialogResult = true;
            Close();
        }

        
    }
}
