using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Data.SQLite;

namespace WifiRanger
{
    /// <summary>
    /// Class which calculates and displays the router's range using a 
    /// modified free path loss formula.
    /// <author>Max Kernchen</author>
    /// <date>05/05/2018</date>
    /// </summary>
    public partial class Coverage : Page
    {
        //Dispatcher timer for displaying an animated percentage 
        //covered without locking the UI
        private DispatcherTimer coverageTimer;
        //the perecent the router covers
        private double percentCoverageVal = 0.0;
        // counter for animated percentage, has to be field 
        //because tick is called many times
        private double counter            = 0.0;
        // area covered by the router.
        private double area               = 0;
        //if the router is high powered, found in the Routers table
        private Boolean highPower = false;
        // static final field which is used to coveret meters to feet
        private static readonly double METERS_TO_FEET = 3.28084;
        //static final field which is the location of the frequency in the powerFreqList
        private static readonly int FREQUENCY_INDEX   = 0;
        //static final field which is the location of the power in the powerFreqList
        private static readonly int POWER_INDEX       = 1;
        //static final field which is the DBM or strength that the signal 
        //is expected to be for low power meters
        private static readonly int LOW_POWER_DBM     = 57;
        //static final field which is the highpower DBM, 
        //this is higher which indicates that highpower can cover more area
        private static readonly int HIGH_POWER_DBM    = 58;
        //the height of a floor in meters
        private static readonly int HEIGHT_METERS_FLOOR = 3;
  
        /// <summary>
        /// Constructor which gets all necessary data from the Routers table 
        /// and the current properties in the application to calculate the percent covered 
        /// </summary>
        public Coverage()
        {
            InitializeComponent();
            //clear back and forward short cuts
            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();
            //get the number of floors 
            int floors = (int)Application.Current.Properties["Floors"];
            // get the location: 0 == center 1 == corner
            int location = (int)Application.Current.Properties["RouterLocation"];
            // get a list of the power and the frequency
            List<long> powerFreqList = this.getRouterData(Application.Current.Properties["SelectedRouter"].ToString());
            // get the current number passed into the area field 
            area = Convert.ToDouble(Application.Current.Properties["Area"].ToString());
        
             // get the distance covered in one direction, using modified free path loss formula 
            double distanceCovered = this.calculateDistance(powerFreqList[POWER_INDEX], powerFreqList[FREQUENCY_INDEX]);
            //get the covered for the entire area, based upon the floors and the location of the router
            percentCoverageVal = this.calulateCoverage(distanceCovered, area, Application.Current.Properties
                ["Unit"].ToString() == "Meter",location == 0,floors);
            //create the dispatach timer for the animated percentage covered
            coverageTimer = new DispatcherTimer();
            coverageTimer.Interval = new TimeSpan(0, 0, 0,0,10);
            coverageTimer.Tick += CoverageTimer_Tick;


        }
        /// <summary>
        /// Tick for DispaterTimer updates the percent value by 1 each 10 milliseconds
        /// </summary>
        /// <param name="sender">where the event came from not used in this case</param>
        /// <param name="e">arguments for the event not used in this case</param>
        private void CoverageTimer_Tick(object sender, EventArgs e)
        {
            counter += 1;
            PercentCoverage.Content = counter + " %";
            if (counter >= percentCoverageVal)
            {
                coverageTimer.Stop();
            }
            //set colors based upon percentage covered
            else if (percentCoverageVal < 30)
                PercentCoverage.Foreground = Brushes.Red;
            else if (percentCoverageVal > 30 && percentCoverageVal < 70)
                PercentCoverage.Foreground = Brushes.Orange;
            else if (percentCoverageVal > 70)
                PercentCoverage.Foreground = Brushes.PaleGreen;
            
        }
        /// <summary>
        /// Free path loss formula which returns 
        /// the distance the router covers in one direction
        /// </summary>
        /// <param name="power">the power of the router in milliwats</param>
        /// <param name="freqInMHz">the frequency of the route rin mhz</param>
        /// <returns></returns>
        private double calculateDistance(double power, double freqInMHz)
        {
            double exp = 0;
            if(!highPower)
                //free path loss formula solved for distance with natual log of power added, 
                //to better estimate based upon real world observations 
                exp = (27.55 - (20 * Math.Log10(freqInMHz)) + LOW_POWER_DBM + Math.Log(power)) / 20.0;
           else
                //add a bit more signal buffer for high power routers 
                exp = (27.55 - (20 * Math.Log10(freqInMHz)) + HIGH_POWER_DBM + Math.Log(power)) / 20.0;

            return Math.Pow(10.0, exp);
        }
        /// <summary>
        //  Turns the distance covered in one direction into a total percent covered of the area in the house
        /// </summary>
        /// <param name="distanceCovered">the distance covered in one direction in meters</param>
        /// <param name="area">the area of the house in sq meters</param>
        /// <param name="sqMeters">if the area is sq meters or sq feet</param>
        /// <param name="nearCenter">if the router is near the center</param>
        /// <param name="numFloors">how many floors are in the house</param>
        /// <returns>a double of the percent covered</returns>
        private double calulateCoverage(double distanceCovered, double area, bool sqMeters, Boolean nearCenter, int numFloors)
        {
            //subtract the 50% height of the floors from the range 
            distanceCovered = distanceCovered - (.5 *(numFloors * HEIGHT_METERS_FLOOR));
            //covert the router range in one direction to feet if area was in sq feet
            if (!sqMeters)
                distanceCovered = distanceCovered * METERS_TO_FEET;
            // get the width and height assuming an average house is 
            // is a 7:10 height width ratio
            double width = (Math.Sqrt(area/70)) * 10;
            double height = (Math.Sqrt(area/70)) * 7;
            // get how much is covered for the width of the house
            double lengthCoverage = distanceCovered / width;

            double coverage = 0.0;
            // if the router is not near the center get the diagnal length of the area
            // by using pythagorean theorem
            if (!nearCenter)
            {
                double hypoCoverage = Math.Sqrt(Math.Pow(height, 2) + Math.Pow(width, 2));
                coverage = (distanceCovered / (hypoCoverage))* 100;
            }
            // if it is near the center then only take into account the coverage of the width, 
            //since it is the largest measurement of the house
            else
                coverage = (lengthCoverage) * 100;

            
            if (coverage > 100)
                return 100;
            else
                return coverage;


        }
        /// <summary>
        /// Once the page loads start the animated percent coverage
        /// </summary>
        /// <param name="sender">where the event came from, not used in this case</param>
        /// <param name="e">arguments for the event not used in this case</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            coverageTimer.Start();
        }
        /// <summary>
        /// Get all the needed data for the coverage page from the Routers table
        /// </summary>
        /// <param name="model">the model which is used as a 
        /// primary key in the Routers database</param>
        /// <returns>a list which contains the power and 
        /// frequency for the specified router model</returns>
        private List<long> getRouterData(String model)
        {
            // connect to the SQLite databse
            SQLiteConnection connection = new SQLiteConnection(Properties.Settings.Default.RoutersDBConnectionString);
            List<long> frequencyPowerList = new List<long>();
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
               
                Debug.WriteLine("Can not open connection! " + ex.ToString());
            }
            // get the frequency and add it to the returned list
            SQLiteCommand cmdFreq = new SQLiteCommand(ConfigurationManager.
                AppSettings["getFrequency"].ToString(), connection);
            cmdFreq.Parameters.AddWithValue("@model", model);
            // have to use long due to dynamic integer typing that SQLite does
            long frequency =  (long) cmdFreq.ExecuteScalar();
            frequencyPowerList.Add(frequency);
            //get the power of the model and add it to the returned list
            SQLiteCommand cmdPower = new SQLiteCommand(ConfigurationManager.
                AppSettings["getPower"].ToString(), connection);
            cmdPower.Parameters.AddWithValue("@model", model);
            long power = (long)cmdPower.ExecuteScalar();
            frequencyPowerList.Add(power);
            //get the image resource name and use to populate the image 
            //in the coverage page
            SQLiteCommand cmdImage = new SQLiteCommand(ConfigurationManager.
                AppSettings["getImage"].ToString(), connection);
            cmdImage.Parameters.AddWithValue("@model", model);
            string imageLocation = (string)cmdImage.ExecuteScalar();

            RouterImage.Source =  this.LoadImage(imageLocation);
            RouterName.Text = model;
            //get the store link and make sure we can set it as a URI 
            SQLiteCommand cmdURL = new SQLiteCommand(ConfigurationManager.
                AppSettings["getID"].ToString(), connection);
            cmdURL.Parameters.AddWithValue("@model", model);
            try
            {
                StoreLink.NavigateUri = new Uri(this.getURL((long)cmdURL.ExecuteScalar()));
            }catch(UriFormatException ufe)
            {
                MessageBox.Show("There is no internet connection, store links will not be available");
                Debug.WriteLine(ufe.Message);
            }
           
            // get if the router is high powered and set the respective boolean
            SQLiteCommand cmdHighPower = new SQLiteCommand(ConfigurationManager.
                AppSettings["getIsHighPower"].ToString(), connection);
            cmdHighPower.Parameters.AddWithValue("@model", model);

            highPower =  (long)cmdHighPower.ExecuteScalar() == 1;
            
            return frequencyPowerList;
        }
        /// <summary>
        /// Navigates to the URI specified which opens up a new window in the default browswer
        /// </summary>
        /// <param name="sender">where the event came from not used in this case</param>
        /// <param name="e">arguments from the event used for the URI in this case</param>
        private void URL_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Length > 0)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                    e.Handled = true;
                }
                catch (System.InvalidOperationException ioe)
                {
                     
                    MessageBox.Show("No Internet Connection Found");
                    Debug.WriteLine(ioe.Message);
                }
            }
        }
        /// <summary>
        /// Request to Walmart's webservice to get the router's URL
        /// </summary>
        /// <param name="itemid">the unquie walmart item id</param>
        /// <returns>a string representation of the routers url</returns>
        private string getURL(long itemid)
        {
            String productUrl = "";
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    //get the JSON response and use our api key
                    String url = "http://api.walmartlabs.com/v1/items/" + itemid + "?format=json&apiKey=" + ConfigurationManager.AppSettings["WalmartKey"].ToString();

                    var json = webClient.DownloadString("http://api.walmartlabs.com/v1/items/" + itemid + "?format=json&apiKey=" + ConfigurationManager.AppSettings["WalmartKey"].ToString());
                    //deserialize using newsoft's JSON.net
                    var results = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
                    productUrl = results["productUrl"];
                }
            }
            catch (System.Net.WebException we)
            { 
                Debug.WriteLine(we.Message);
            }
            return productUrl;
        }

        /// <summary>
        /// Helper method which turns a URI path into a bitmap image
        /// </summary>
        /// <param name="filename">the image file name from the Routers database</param>
        /// <returns>BitMapImage for the image files in the resources folder</returns>
        private BitmapImage LoadImage(string filename)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/" + filename));
        }

        /// <summary>
        /// Start over button event handler which sends the user back to the Routers page
        /// </summary>
        /// <param name="sender">where the event came from, not used in this case</param>
        /// <param name="e">arguments for the event, not used in this case</param>
        private void startOverBtn_Click(object sender, RoutedEventArgs e)
        {
            
            this.NavigationService.Navigate(new Uri("Routers.xaml", UriKind.Relative));
        }

    }
}
