using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;
using System.Xml;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;

namespace WifiRanger
{
    /// <summary>
    /// Interaction logic for Routers.xaml
    /// </summary>
    public partial class Routers : Page
    {
        //number of rows in router database
        private int numRows;
        // the column that contains the Router Name
        private static readonly int ROUTER_NAME = 1;
        // the column that contains the Router Model name and number
        private static readonly int ROUTER_MODEL = 2;
        // the column that contains the Router Model name and number
        private static readonly int ROUTER_ITEM_ID = 4;
        // the column that contains the name of the ImageFile for the router
        private static readonly int ROUTER_IMAGE = 6;
        // location of Router table in router database
        private static readonly int ROUTER_TABLE = 0;

        private bool connectedToInternet = false;

        public Routers()
        {
            InitializeComponent();
            DataSet routerDS = this.getRouterData();
            DataRow routerDR;
            numRows = routerDS.Tables[0].Rows.Count;
            RouterData[] routerDataArray = new RouterData[numRows];
            for (int i = 0; i < numRows; i++)
            {
                routerDR = routerDS.Tables[ROUTER_TABLE].Rows[i];
                routerDataArray[i] = new RouterData
                {
                    Name = routerDR.ItemArray.GetValue(ROUTER_NAME).ToString(),
                    ImageData = LoadImage(routerDR.ItemArray.GetValue(ROUTER_IMAGE).ToString()),
                    Model = routerDR.ItemArray.GetValue(ROUTER_MODEL).ToString(),
                    Price = this.getPrice(routerDR.ItemArray.GetValue(ROUTER_ITEM_ID).ToString()),
                    URL = this.getURL(routerDR.ItemArray.GetValue(ROUTER_ITEM_ID).ToString())
                };
            }


            RouterList.ItemsSource = routerDataArray;
            String url = "enterwalmartapiurlhere";
            /**
                        XmlTextReader reader = new XmlTextReader(url);
                        String prevElement = "";

                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {

                                case XmlNodeType.Element: // The node is an element.
                                    //Console.Write(reader.Name);
                                    prevElement = reader.Name;
                                    break;
                                case XmlNodeType.Text: //Display the text in each element.
                                    if(prevElement == "msrp")
                                        Console.WriteLine(reader.Value + " value");
                                    break;

                            }
                        }

                        */
                        /*
            using (var webClient = new System.Net.WebClient())
            {
                var json = webClient.DownloadString("http://api.walmartlabs.com/v1/items/46927632?format=json&apiKey=enterkeyhere");
                var results = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
                Console.WriteLine(results["msrp"]);
                }
                */
            Console.WriteLine(this.calculateDistance(-60, 2400));
        }
        
        /**
         * Opens the Image from the resources folder
            @param: String:filename - the name of the file inside the resources folder
            @returns: A new BitmapImage
 
         */

        private BitmapImage LoadImage(string filename)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/" + filename));
        }

        private void RouterItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RouterData routerData = (RouterData)RouterList.SelectedItem;
            Console.WriteLine(routerData.Name);
        }

        private DataSet getRouterData()
        {
            SqlConnection connection = new SqlConnection(Properties.Settings.Default.RoutersDBConnectionString);

            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                //will need to go to log file later
                Console.WriteLine("Can not open connection! " + ex.ToString());
            }

            SqlDataAdapter sqlData = new SqlDataAdapter(Properties.Settings.Default.allRouters, connection);
            DataSet routerDataSet = new DataSet();
            // put the data in the dataset and source it back to the Routers table
            sqlData.Fill(routerDataSet, "Routers");

            return routerDataSet;
        }

        private double calculateDistance(double levelInDb, double freqInMHz)
        {
            double exp = (27.55 - (20 * Math.Log10(freqInMHz)) + Math.Abs(levelInDb)) / 20.0;
            // times 2 to get a better reading, after real world tests. 
            return 2 * Math.Pow(10.0, exp);
        }
        private string getPrice(string itemid)
        {
            double price = 0.0;
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    String url = "http://api.walmartlabs.com/v1/items/" + itemid + "?format=json&apiKey=" + ConfigurationManager.AppSettings["WalmartKey"].ToString();
                    Console.WriteLine(url);
                    var json = webClient.DownloadString("http://api.walmartlabs.com/v1/items/"+ itemid+"?format=json&apiKey="+ ConfigurationManager.AppSettings["WalmartKey"].ToString());
                    var results = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
                    price = results["salePrice"];
                }
            }catch(System.Net.WebException we)
            {
                Console.WriteLine(we.ToString());
                connectedToInternet = false;
                return "No Price Available";
            }
            connectedToInternet = true;
            return System.Convert.ToString(price);
        }
        private string getURL(string itemid)
        {
            String productUrl = "";
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    String url = "http://api.walmartlabs.com/v1/items/" + itemid + "?format=json&apiKey=" + ConfigurationManager.AppSettings["WalmartKey"].ToString();
                    Console.WriteLine(url);
                    var json = webClient.DownloadString("http://api.walmartlabs.com/v1/items/" + itemid + "?format=json&apiKey=" + ConfigurationManager.AppSettings["WalmartKey"].ToString());
                    var results = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
                    productUrl = results["productUrl"];
                }
            }
            catch (System.Net.WebException we)
            {
                connectedToInternet = false;
                Console.WriteLine(we.ToString());
            }
            connectedToInternet = true;
            return productUrl;
        }

        private void URL_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }catch(System.InvalidOperationException ioe)
            {
                MessageBox.Show("No Internet Connection Found");
            }
        }
    }

   
}

