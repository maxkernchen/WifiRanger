using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
        private double counter            = 0.0;
        private double area               = 0;
     
        private static readonly double METERS_TO_FEET = 3.28084;
        private static readonly int FREQUENCY_INDEX   = 0;
        private static readonly int POWER_INDEX       = 1;
        //the height of a floor in meters
        private static readonly int HEIGHT_METERS_FLOOR = 3;
        private static readonly int NEAR_CENTER = 0;
        private static readonly int NEAR_CORNER = 1;
        public Floors()
        {
            InitializeComponent();
            Console.WriteLine("init called");
            int floors = (int) Application.Current.Properties["Floors"];
            Console.WriteLine(Application.Current.Properties["Unit"]);
            Console.WriteLine(Application.Current.Properties["RouterLocation"]);
            int location = (int) Application.Current.Properties["RouterLocation"];
            
            List<int> powerFreqList = this.getRouterFrequencyPower(Application.Current.Properties["SelectedRouter"].ToString());
            area = Convert.ToDouble(Application.Current.Properties["Area"].ToString());
          
            
            double distanceCovered = this.calculateDistance(powerFreqList[POWER_INDEX],powerFreqList[FREQUENCY_INDEX]);
            Console.WriteLine(distanceCovered);
            Console.WriteLine(this.calulateCoverage(distanceCovered, area, Application.Current.Properties
                ["Unit"].ToString()=="Meter",location,floors));
            percentCoverageVal = this.calulateCoverage(distanceCovered, area, Application.Current.Properties
                ["Unit"].ToString() == "Meter",location,floors);
            coverageTimer = new DispatcherTimer();
            coverageTimer.Interval = new TimeSpan(0, 0, 0,0,10);
            coverageTimer.Tick += CoverageTimer_Tick;





        }

        private void CoverageTimer_Tick(object sender, EventArgs e)
        {
            counter += 1;
            PercentCoverage.Content = counter + " %";
            if (counter >= percentCoverageVal)
            {
                coverageTimer.Stop();
            }
            else if (percentCoverageVal < 30)
                PercentCoverage.Foreground = Brushes.Red;
            else if (percentCoverageVal > 30 && percentCoverageVal < 70)
                PercentCoverage.Foreground = Brushes.Orange;
            else if (percentCoverageVal > 70)
                PercentCoverage.Foreground = Brushes.PaleGreen;
            
        }

        /**
         * Returns meter's range in one direction 
         */
        private double calculateDistance(double power, double freqInMHz)
        {
            double exp = (27.55 - (20 * Math.Log10(freqInMHz)) + 10 * Math.Log(power)) / 20.0;
           
            return Math.Pow(10.0, exp);
        }
        private double calulateCoverage(double distanceCovered, double area, bool sqMeters, int location, int numFloors)
        {
            //subtract the height of each floor from the range 
            distanceCovered = distanceCovered - (numFloors * HEIGHT_METERS_FLOOR);
            if (!sqMeters)
                distanceCovered = distanceCovered * METERS_TO_FEET;

            double length = (Math.Sqrt(area/70)) * 10;
            double width = (Math.Sqrt(area/70)) * 7;

            double lengthCoverage = distanceCovered / (length + (numFloors * HEIGHT_METERS_FLOOR)/2);
            double coverage = 0.0;
            if(location == NEAR_CORNER)
            {
                double hypoCoverage = Math.Sqrt(Math.Pow(length, 2) + Math.Pow(width, 2));
                coverage = (distanceCovered / (hypoCoverage + (numFloors * HEIGHT_METERS_FLOOR)/2)) * 100;
            } else
                coverage = (lengthCoverage) * 100;

            
            if (coverage > 100)
                return 100;
            else
                return coverage;


        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            coverageTimer.Start();
        }

        private List<int> getRouterFrequencyPower(String model)
        {
            SqlConnection connection = new SqlConnection(Properties.Settings.Default.RoutersDBConnectionString);
            List<int> frequencyPowerList = new List<int>();
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
               
                Console.WriteLine("Can not open connection! " + ex.ToString());
            }

            SqlCommand cmdFreq = new SqlCommand(ConfigurationManager.AppSettings["getFrequency"].ToString(), connection);
            cmdFreq.Parameters.AddWithValue("@model", model);
            int frequency = (int) cmdFreq.ExecuteScalar();
            frequencyPowerList.Add(frequency);
            SqlCommand cmdPower = new SqlCommand(ConfigurationManager.AppSettings["getPower"].ToString(), connection);
            cmdPower.Parameters.AddWithValue("@model", model);
            int power = (int)cmdPower.ExecuteScalar();
            frequencyPowerList.Add(power);
            return frequencyPowerList;
        }
    }
}
