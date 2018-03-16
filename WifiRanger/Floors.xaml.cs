using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace WifiRanger
{
    /// <summary>
    /// Interaction logic for Floors.xaml
    /// </summary>
    public partial class Floors : Page
    {
        private DispatcherTimer coverageTimer;
        private double percentCoverageVal = 0.0;
        public Floors()
        {
            InitializeComponent();
            Console.WriteLine("init called");
            Console.WriteLine(Application.Current.Properties["Floors"]);
            coverageTimer = new DispatcherTimer();
            coverageTimer.Interval = new TimeSpan(0, 0, 0,0,10);
            coverageTimer.Tick += CoverageTimer_Tick;





        }

        private void CoverageTimer_Tick(object sender, EventArgs e)
        {
            percentCoverageVal += 1;
            PercentCoverage.Content = percentCoverageVal+ " %";
            if (percentCoverageVal < 30)
                PercentCoverage.Foreground = Brushes.Red;
            if (percentCoverageVal > 30 && percentCoverageVal < 70)
                PercentCoverage.Foreground = Brushes.Orange;
            if (percentCoverageVal > 70)
                PercentCoverage.Foreground = Brushes.PaleGreen;
            if (percentCoverageVal >= 100)
            {
                coverageTimer.Stop();
            }
        }

        

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            coverageTimer.Start();
        }
    }
}
