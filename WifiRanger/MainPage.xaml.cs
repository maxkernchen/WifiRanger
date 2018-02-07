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

namespace WifiRanger
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
       private int FloorsVal = 0;
        public MainPage()
        {
            InitializeComponent();
        }

        private void Calculate_Button_Click(object sender, RoutedEventArgs e)
        {

            if(Floors.SelectedItem != null)
            {
                FloorsVal = int.Parse(Floors.Text.ToString());
                
                Application.Current.Properties["Floors"] = FloorsVal;
            }
            this.NavigationService.Navigate(new Uri("Floors.xaml", UriKind.Relative));
        }
    }
}
