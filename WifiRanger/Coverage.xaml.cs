using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
        private Boolean highPower = false;
     
        private static readonly double METERS_TO_FEET = 3.28084;
        private static readonly int FREQUENCY_INDEX   = 0;
        private static readonly int POWER_INDEX       = 1;
        private static readonly int LOW_POWER_DBM     = 57;
        private static readonly int HIGH_POWER_DBM    = 58;
        //the height of a floor in meters
        private static readonly int HEIGHT_METERS_FLOOR = 3;
        private static readonly int NEAR_CENTER         = 0;
        private static readonly int NEAR_CORNER         = 1;
        public Floors()
        {
            InitializeComponent();
            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();
            Console.WriteLine("init called");
            int floors = (int)Application.Current.Properties["Floors"];
            Console.WriteLine(Application.Current.Properties["Unit"]);
            Console.WriteLine(Application.Current.Properties["RouterLocation"]);
            int location = (int)Application.Current.Properties["RouterLocation"];

            List<int> powerFreqList = this.getRouterData(Application.Current.Properties["SelectedRouter"].ToString());
            area = Convert.ToDouble(Application.Current.Properties["Area"].ToString());

            double power = this.milliWatt_To_Dbm(powerFreqList[POWER_INDEX]);

            double distanceCovered = this.calculateDistance(powerFreqList[POWER_INDEX], powerFreqList[FREQUENCY_INDEX]);
            Console.WriteLine(distanceCovered);
            Console.WriteLine(this.calulateCoverage(distanceCovered, area, Application.Current.Properties
                ["Unit"].ToString()== "Meter",location,floors));
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
            double exp = 0;
            if(!highPower)
                //free path loss formula with natual log of power added, to better estimate based upon real world observations 
                exp = (27.55 - (20 * Math.Log10(freqInMHz)) + LOW_POWER_DBM + Math.Log(power)) / 20.0;
           else
                //add a bit more signal buffer for high power routers 
                exp = (27.55 - (20 * Math.Log10(freqInMHz)) + HIGH_POWER_DBM + Math.Log(power)) / 20.0;

            return Math.Pow(10.0, exp);
        }
        private double calulateCoverage(double distanceCovered, double area, bool sqMeters, int location, int numFloors)
        {
            //subtract the 50% height of the floors from the range 
            distanceCovered = distanceCovered - (.5 *(numFloors * HEIGHT_METERS_FLOOR));
            if (!sqMeters)
                distanceCovered = distanceCovered * METERS_TO_FEET;

            double length = (Math.Sqrt(area/70)) * 10;
            double width = (Math.Sqrt(area/70)) * 7;

            double lengthCoverage = distanceCovered / (length);
            double coverage = 0.0;
            if(location == NEAR_CORNER)
            {
                double hypoCoverage = Math.Sqrt(Math.Pow(length, 2) + Math.Pow(width, 2));
                coverage = (distanceCovered / (hypoCoverage))* 100;
            }
            else
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

        private List<int> getRouterData(String model)
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

            SqlCommand cmdImage = new SqlCommand(ConfigurationManager.AppSettings["getImage"].ToString(), connection);
            cmdImage.Parameters.AddWithValue("@model", model);
            string imageLocation = (string)cmdImage.ExecuteScalar();

            RouterImage.Source =  this.LoadImage(imageLocation);
            RouterName.Text = model;

            SqlCommand cmdURL = new SqlCommand(ConfigurationManager.AppSettings["getID"].ToString(), connection);
            cmdURL.Parameters.AddWithValue("@model", model);
            StoreLink.NavigateUri = new Uri(this.getURL((int)cmdURL.ExecuteScalar()));

            SqlCommand cmdHighPower = new SqlCommand(ConfigurationManager.AppSettings["getIsHighPower"].ToString(), connection);
            cmdHighPower.Parameters.AddWithValue("@model", model);

            highPower = (Byte)cmdHighPower.ExecuteScalar() == 1;
            

            return frequencyPowerList;
        }

        private void URL_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch (System.InvalidOperationException ioe)
            {
               //send to log file in future.
                MessageBox.Show("No Internet Connection Found");
            }
        }
        private string getURL(int itemid)
        {
            String productUrl = "";
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    String url = "http://api.walmartlabs.com/v1/items/" + itemid + "?format=json&apiKey=" + ConfigurationManager.AppSettings["WalmartKey"].ToString();

                    var json = webClient.DownloadString("http://api.walmartlabs.com/v1/items/" + itemid + "?format=json&apiKey=" + ConfigurationManager.AppSettings["WalmartKey"].ToString());
                    var results = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
                    productUrl = results["productUrl"];
                }
            }
            catch (System.Net.WebException we)
            { 
                Console.WriteLine(we.ToString());
            }
            return productUrl;
        }


        private BitmapImage LoadImage(string filename)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/" + filename));
        }


        private void startOverBtn_Click(object sender, RoutedEventArgs e)
        {
            
            this.NavigationService.Navigate(new Uri("Routers.xaml", UriKind.Relative));
        }

        private double milliWatt_To_Dbm(double milliWatts)
        {
            return 10 * Math.Log10(milliWatts / 1);
        }
    }
}
