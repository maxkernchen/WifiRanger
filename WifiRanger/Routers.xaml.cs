using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Configuration;
using System.Data;
using Newtonsoft.Json;
using System.Diagnostics;

namespace WifiRanger
{
    /// <summary>
    /// Class which initializes the list of routers on the home screen
    /// gets all data from local database 'routersdb.mdf' table - 'Routers'
    /// or from web service calls to Walmart's open API devoloper web service 
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
        //the last column which was sorted
        private string lastSorted = "";
        // boolean for switching sorting direcotions
        private bool sortSwitch = false;
        // list for columns which are sortable
        private static readonly string[] sortableColumns = { "Model", "Brand", "Current Price",
                                                             };
        // array of RouterData objects, these old all fields related to one router
        private RouterData[] routerData;

        /// <summary>
        /// Construtor which gets the all the routers from the Routers table
        /// and sets them to list displayed
        /// </summary>
        public Routers()
        {
            InitializeComponent();
            DataSet routerDS = this.getRouterData();
            this.initalizeRouterList(routerDS);

            //remove all back and forwards keyboard shortcuts
            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();

        }

       /// <summary>
       /// Loads an image from a file name
       /// </summary>
       /// <param name="filename"> the name of the image in the resources folder</param>
       /// <returns>A BitmapImage which can be used within the router list in the UI </returns>

        private BitmapImage LoadImage(string filename)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/" + filename));
        }
        /// <summary>
        /// Button listener for when an item in the router list is clicked
        /// The selected item will then be stored in the Appilcation-Scope Properties.
        /// </summary>
        /// <param name="sender">object which the event came from</param>
        /// <param name="e">Extra arguments from the event, not used here</param>
        private void RouterItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
         
            RouterData routerData = RouterList.SelectedItem as RouterData;
            Application.Current.Properties["SelectedRouter"] = routerData.Model;
            // go to the main page
            this.NavigationService.Navigate(new Uri("MainPage.xaml", UriKind.Relative));

        }
        /// <summary>
        /// Helper method to get all the routers in the Router table
        /// </summary>
        /// <returns>a SQL DataSet with all the routers in it</returns>
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
                //catch when the exception if the local database is missing
                Debug.WriteLine("Cannnot connect to local db" + ex.Message);

            }

            SqlDataAdapter sqlData = new SqlDataAdapter(Properties.Settings.Default.allRouters, connection);
            // put the data in the dataset and source it back to the Routers table
            sqlData.Fill(returnedDS, "Routers");
            
            return returnedDS;
        }
        /// <summary>
        /// Gets the price by calling the Walmart web service for the given item id
        /// </summary>
        /// <param name="itemid">the itemid of the router</param>
        /// <returns>a string representation of the price</returns>
        private string getPrice(string itemid)
        {
            double price = 0.0;
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    //connection url for web service with api key passed in from App.config file

                    String url = "http://api.walmartlabs.com/v1/items/" + itemid + "?format=json&apiKey=" 
                        + ConfigurationManager.AppSettings["WalmartKey"].ToString();
                    //json request sent to Walmart web service
                    var request = webClient.DownloadString(url);
                    var results = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(request);
                    //get the current sales price
                    price = results["salePrice"];
                }
            }
            //catch any web service or network errors
            catch (System.Net.WebException we)
            {
                Debug.WriteLine(we.ToString());
                return "No Price Available";
            }
            return System.Convert.ToString(price);
        }
        /// <summary>
        /// Gets the walmart page url for the given item id of the router
        /// </summary>
        /// <param name="itemid">the item id for the router</param>
        /// <returns>the url page for the router</returns>
        private string getURL(string itemid)
        {
            String productUrl = "";
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    //url to walmart web service
                    String url = "http://api.walmartlabs.com/v1/items/" + itemid + "?format=json&apiKey="
                        + ConfigurationManager.AppSettings["WalmartKey"].ToString();
                    //pass url to json request and await results
                    var request = webClient.DownloadString(url);
                    var results = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(request);
                    //get the product url 
                    productUrl = results["productUrl"];
                }
            }
            // catch any webservice or network exceptions 
            catch (System.Net.WebException we)
            {
               
                Debug.WriteLine(we.ToString());
            }
           
            return productUrl;
        }
        /// <summary>
        /// Helper method for navigation to a url
        /// </summary>
        /// <param name="sender">object which invoke the event, not used here</param>
        /// <param name="e">arguments for the event of requesting navigation to the 
        /// url</param>
        private void URL_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                //go the the URI defined in the arguments
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            //catch errors if there is no internet connection
            catch (System.InvalidOperationException ioe)
            {
                Debug.WriteLine(ioe.Message);
                MessageBox.Show("No Internet Connection Found");
            }
        }
        /// <summary>
        /// Listen for when the search text is changed, 
        /// if it is changed refresh the list for the given search terms
        /// </summary>
        /// <param name="sender">object the event came from, not used here</param>
        /// <param name="e">Arguments from the event, not used here</param>
        private void SearchRouters_TextChanged(object sender, TextChangedEventArgs e)
        {
            //reset the sorting if it was done
            this.resetSorting();
            //if there is no search terms just display all routers
            if (SearchRouters.Text.Length == 0)
                RouterList.ItemsSource = this.routerData;
            else
                //search all routers for given term
                this.searchAndUpdateRouters(SearchRouters.Text.ToLower());
        }
        /// <summary>
        /// Helper method which turns the SQL dataset into the list displayed in the UI.
        /// </summary>
        /// <param name="routerDS">The SQL dataset gotten from the Routers table</param>
        private void initalizeRouterList(DataSet routerDS)
        {
            DataRow routerDR;
            // covert each data row into a new RouterData object
            numRows = routerDS.Tables[0].Rows.Count;
            RouterData[] routerDataArray = new RouterData[numRows];
            for (int i = 0; i < numRows; i++)
            {
                //get all 5 columns which are used in UI from the data set
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

            // set the WPF list to the array of router data objects
            RouterList.ItemsSource = routerDataArray;
            this.routerData = routerDataArray;
        }
        /// <summary>
        /// search from the routerList and find only routers whos brand or model contain the search terms
        /// </summary>
        /// <param name="searchTerm">the search terms to look for</param>
        private void searchAndUpdateRouters(string searchTerm)
        {
            List<RouterData> searchResults = new List<RouterData>();
            for (int i = 0; i < routerData.Length; i++)
            {
                // Brand or Model contains the search term add it to he result list
                if (routerData[i].Name.ToLower().Contains(searchTerm) || routerData[i].Model.ToLower().Contains(searchTerm))
                {
                    searchResults.Add(routerData[i]);
                }
            }
            // set the WPF list to the results
            RouterList.ItemsSource = searchResults;
        }
        /// <summary>
        /// Event listener for when a column is clicked for sorting
        /// </summary>
        /// <param name="sender">object which the event came from</param>
        /// <param name="e">arguments from the event in this case used to get the correct source</param>
        private void Column_Click(object sender, RoutedEventArgs e)
        {
            //clear searches 
            SearchRouters.Clear();
            GridViewColumnHeader columnClicked = e.OriginalSource as GridViewColumnHeader;
            if (columnClicked == null)
                return;

            // get only name of column sorted
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
                    this.resetSorting();
                                    
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
        /// <summary>
        /// Helper method which sorts the routers using Orderby method for a list
        /// </summary>
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

            }
            else if(lastSorted == "Model")
            {
                if (!sortSwitch)     
                    //sort for each model name
                    RouterList.ItemsSource = routerData.OrderBy(obj => obj.Model).ToList();
                else
                    RouterList.ItemsSource = routerData.OrderByDescending(obj => obj.Model).ToList();

            }
            else if(lastSorted == "Current Price")
            {
                if (!sortSwitch)
                {                   
                    //sort for current price, but remove items with no price.
                    List<RouterData> removeNoPrice = routerData.ToList();
                    removeNoPrice.RemoveAll(data => data.Price == "No Price Available");

                    //need to convert to double for accurate sorting
                    removeNoPrice = removeNoPrice.OrderBy(d =>Double.Parse(d.Price)).ToList();
                    RouterList.ItemsSource = removeNoPrice;
                }
                else
                {
                    List<RouterData> removeNoPrice = routerData.ToList();
                    removeNoPrice.RemoveAll(data => data.Price == "No Price Available");
                    removeNoPrice = removeNoPrice.OrderByDescending(d => Double.Parse(d.Price)).ToList();
                    RouterList.ItemsSource = removeNoPrice;
                }
                  
            }

        }
        /// <summary>
        /// Resets all sorting done and removes the arrows from the columns
        /// </summary>
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

