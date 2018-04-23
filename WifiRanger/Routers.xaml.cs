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

        private string lastSorted = "";
        private bool sortSwitch = false;
        private static readonly string[] sortableColumns = { "Model", "Brand", "Current Price",
                                                             };

        private RouterData[] routerData;

        public Routers()
        {
            InitializeComponent();
            DataSet routerDS = this.getRouterData();
            this.initalizeRouterList(routerDS);


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
            Console.WriteLine(sender.GetType());

            Console.WriteLine("Selected index " + RouterList.SelectedIndex);
            RouterData routerData = RouterList.SelectedItem as RouterData;
            Application.Current.Properties["SelectedRouter"] = routerData.Model;
            Console.WriteLine(routerData.Model);
            this.NavigationService.Navigate(new Uri("MainPage.xaml", UriKind.Relative));

        }

        private DataSet getRouterData()
        {
            SqlConnection connection = new SqlConnection(Properties.Settings.Default.RoutersDBConnectionString);
            DataSet returnedDS = new DataSet();
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                //will need to go to log file later
                Console.WriteLine("Can not open connection! " + ex.ToString());
            }
            /* if (search.Length > 0)
             {



                 SqlCommand cmdSearch = new SqlCommand(Properties.Settings.Default.searchRouters, connection);
                 cmdSearch.Parameters.AddWithValue("@search", search);

                 SqlDataAdapter sqlDASearch = new SqlDataAdapter();
                 sqlDASearch.SelectCommand = cmdSearch;
                 sqlDASearch.Fill(returnedDS,"Routers");

             }
             else
             {*/
            SqlDataAdapter sqlData = new SqlDataAdapter(Properties.Settings.Default.allRouters, connection);

            // put the data in the dataset and source it back to the Routers table
            sqlData.Fill(returnedDS, "Routers");
            //}
            return returnedDS;
        }

        private string getPrice(string itemid)
        {
            double price = 0.0;
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    String url = "http://api.walmartlabs.com/v1/items/" + itemid + "?format=json&apiKey=" + ConfigurationManager.AppSettings["WalmartKey"].ToString();

                    var json = webClient.DownloadString("http://api.walmartlabs.com/v1/items/" + itemid + "?format=json&apiKey=" + ConfigurationManager.AppSettings["WalmartKey"].ToString());
                    var results = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
                    price = results["salePrice"];
                }
            }
            catch (System.Net.WebException we)
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
            }
            catch (System.InvalidOperationException ioe)
            {
                MessageBox.Show("No Internet Connection Found");
            }
        }

        private void SearchRouters_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.resetSorting();
            if (SearchRouters.Text.Length == 0)
                RouterList.ItemsSource = this.routerData;
            else
                this.searchAndUpdateRouters(SearchRouters.Text.ToLower());

        }
        private void initalizeRouterList(DataSet routerDS)
        {
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
            this.routerData = routerDataArray;
        }

        private void searchAndUpdateRouters(string searchTerm)
        {
            List<RouterData> temp = new List<RouterData>();
            for (int i = 0; i < routerData.Length; i++)
            {
                if (routerData[i].Name.ToLower().StartsWith(searchTerm) || routerData[i].Model.ToLower().StartsWith(searchTerm))
                {
                    temp.Add(routerData[i]);
                }
            }

            RouterList.ItemsSource = temp;
        }
        
        private void Column_Click(object sender, RoutedEventArgs e)
        {
            //clear searches 
            SearchRouters.Clear();
            GridViewColumnHeader columnClicked = e.OriginalSource as GridViewColumnHeader;
            if (columnClicked == null)
                return;

            Console.WriteLine(columnClicked.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last());
            string column = columnClicked.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            //get rid of the arrows if needed
            if (column.Contains("\u25BC"))
                column = column.Split(new string[] { " \u25BC" }, StringSplitOptions.None).First();
            else if (column.Contains("\u25B2"))
                column = column.Split(new string[] { " \u25B2" }, StringSplitOptions.None).First();
            // see if column is sortable
            if (sortableColumns.Contains<string>(column))
            {
                // check to see if clicked column is different from last sorted column
                if (lastSorted != column && lastSorted.Length > 0)
                {
                    this.resetSorting();
                    
                }
                //update late sorted column field
                this.lastSorted = column;
                //find the header for the clicked column by going through same process as above
                GridViewColumn tempColumn = null;
                GridView tempGridView = (GridView)RouterList.View;
                GridViewColumnCollection columns = tempGridView.Columns;
                foreach (GridViewColumn columnGridView in columns)
                {
                    if (columnGridView.Header.ToString().Contains(column))
                    {
                        tempColumn = columnGridView;
                        break;
                    }

                }
                //switch sorting order for clicked column
                if (sortSwitch)
                    tempColumn.Header = column + " \u25BC";
                else
                    tempColumn.Header = column + " \u25B2";
                sortSwitch = !sortSwitch;

                this.sortRouters();

            }
        }

        private void sortRouters()
        {
          
            if (lastSorted == "Brand")
            {
                if (!sortSwitch)
                     //lambda for each name field
                    RouterList.ItemsSource = routerData.OrderBy(obj => obj.Name).ToList();                
                else                
                    //go other way
                    RouterList.ItemsSource = routerData.OrderByDescending(obj => obj.Name).ToList();

            }else if(lastSorted == "Model")
            {
                if (!sortSwitch)                   
                    RouterList.ItemsSource = routerData.OrderBy(obj => obj.Model).ToList();
                else
                    RouterList.ItemsSource = routerData.OrderByDescending(obj => obj.Model).ToList();

            }else if(lastSorted == "Current Price")
            {
                if (!sortSwitch)
                {
                    List<RouterData> removeNoPrice = routerData.OrderBy(obj => obj.Price).ToList();
                    removeNoPrice.RemoveAll(data => data.Price == "No Price Available");
                    RouterList.ItemsSource = removeNoPrice;
                }

                else
                {
                    List<RouterData> removeNoPrice = routerData.OrderByDescending(obj => obj.Price).ToList();
                    removeNoPrice.RemoveAll(data => data.Price == "No Price Available");
                    RouterList.ItemsSource = removeNoPrice;
                }
                   

            }

        }

        private void resetSorting()
        {
            //get the gridview and then the gridviewcolumns
            GridView tempGridViewClear = (GridView)RouterList.View;
            GridViewColumnCollection columnsClear = tempGridViewClear.Columns;
            //reset all gridviewcolumn headers to be without arrows
            foreach (GridViewColumn columnGridViewClear in columnsClear)
            {

                //get header string
                string tempColumnClear = columnGridViewClear.Header.ToString();
                //remove the arrows 
                if (tempColumnClear.Contains("\u25BC"))
                    tempColumnClear = tempColumnClear.Split(new string[] { " \u25BC" }, StringSplitOptions.None).First();
                else if (columnGridViewClear.Header.ToString().Contains("\u25B2"))
                    tempColumnClear = tempColumnClear.Split(new string[] { " \u25B2" }, StringSplitOptions.None).First();
                //reset all columns to orginal state
                columnGridViewClear.Header = tempColumnClear;
                //reset sorting to ascending
                this.sortSwitch = false;
            }
        }
    }
   
}

