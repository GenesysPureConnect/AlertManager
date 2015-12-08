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
using AlertManager.viewmodel;

namespace AlertManager.view
{
    /// <summary>
    /// Interaction logic for AlertSetSummaryView.xaml
    /// </summary>
    public partial class AlertSetSummaryView : UserControl
    {

        public AlertSetSummaryView()
        {
            InitializeComponent();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var alertSetViewModel = button.DataContext as AlertSetSummaryViewModel;
                if (alertSetViewModel == null) return;
                MainViewModel.Instance.Delete(alertSetViewModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
