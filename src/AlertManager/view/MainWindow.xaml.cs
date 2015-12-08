using System;
using System.Windows;
using AlertManager.viewmodel;

namespace AlertManager.view
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        private void CleanUp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.CleanUp();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.IsConnected)
                    ViewModel.Disconnect();
                else
                    ViewModel.Connect(txtUser.Text, txtPassword.Password, false, txtServer.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
