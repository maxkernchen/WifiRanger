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
    /// Interaction logic for Floors.xaml
    /// </summary>
    public partial class Floors : Page
    {
        public Floors()
        {
            InitializeComponent();
            Console.WriteLine("init called");
            Console.WriteLine(Application.Current.Properties["Floors"]);



        }


        public void Calculate()
        {
            double percentCoverageVal = 0.0;
            while (percentCoverageVal < 100)
            {
                PercentCoverage.Content = percentCoverageVal;
                percentCoverageVal += .5;
                System.Threading.Thread.Sleep(50);
            }

        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Floors.Instance.WindowLoadedCommand.Execute(null);
        }
        private void rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
