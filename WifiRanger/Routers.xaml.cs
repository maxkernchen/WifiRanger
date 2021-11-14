using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Configuration;
using System.Data.SQLite;
using System.Data;
using Newtonsoft.Json;
using System.Diagnostics;
using RestSharp;
using System.Collections;
using System.ComponentModel;
using System.Threading;

namespace WifiRanger
{
    /// <summary>
    /// Class which initializes the list of routers on the home screen
    /// gets all data from local database 'routersdb.mdf' table - 'Routers'
    /// or from web service calls to Walmart's open API devoloper web service 
    /// <author>Max Kernchen</author>
    /// <date>10/21/2021</date>
    /// </summary>
    public partial class Routers : Page
    {
        //number of rows in router database
        private int numRows;
        // the column that contains the Router Brand
        public static readonly int ROUTER_BRAND = 0;
        // the column that contains the Router Model name and number
        public static readonly int ROUTER_MODEL = 1;
        // the column that contains the Router Frequency
        public static readonly int ROUTER_FREQUENCY = 2;
        // the column that contains the Router Model name and number
        public static readonly int ROUTER_ITEM_ID = 3;
        // the column that contains the name of the ImageFile for the router
        public static readonly int ROUTER_IMAGE = 5;
        // the column that contains the name of the ImageFile for the router
        public static readonly int ROUTER_POWER = 6;
        // the column that contains the name of the ImageFile for the router
        public static readonly int ROUTER_IS_HIGH_POWER = 7;
        // location of Router table in router database
        private static readonly int ROUTER_TABLE = 0;
        // index of api price response
        private static readonly int API_PRICE_INDEX = 0;
        // index of api url response
        private static readonly int API_URL_INDEX = 1;
        // index of rating url response
        private static readonly int API_RATING_INDEX = 2;
        //the last column which was sorted
        private string lastSorted = "";
        // boolean for switching sorting direcotions
        private bool sortSwitch = false;
        // list for columns which are sortable
        private static readonly string[] sortableColumns = { "Model", "Brand", "Current Price", 
            "Rating"};
        // array of RouterData objects, this list holds all fields related to one router. 
        // Static so Coverage class can access it without refetching from DB.
        private static List<RouterData> routerDataList;
        // default value for price if api does not return anything
        private static readonly string DEFAULT_PRICE_STR = "0.0";
        // default value for url if api does not return anything,
        // public so it can be found by classes which navigiate to hyperlink
        public static readonly string DEFAULT_URL_STR = "Could Not Find URL";
        // default value for rating if api does not return anything
        private static readonly string DEFAULT_RATING_STR = "0.0";
        // loading message when we retreiving from api in background, public so coverage class 
        // can make sure if a URL has been found yet
        public static readonly string LOADING_API_DATA = "Loading...";
        //background worker, this allows our app to open right away. But the API data is later
        //filled in.
        private readonly BackgroundWorker worker = new BackgroundWorker();
        // private dict for storing our api results, can have null values if API request did not
        // complete 
        private Dictionary<string, ArrayList> allRouterApiResultsDict;
        // how many ms to sleep before next api call/
        private static readonly int API_SLEEP_MS = 500;

        /// <summary>
        /// Construtor which gets the all the routers from the Routers table
        /// and sets them to list displayed
        /// </summary>
        public Routers()
        {
            InitializeComponent();
            DataSet routerDS = this.getRouterData();
            this.initalizeRouterList(routerDS);

            // get api results in background so UI can open immediately 
            worker.DoWork += workerGetApiRouterData;
            worker.RunWorkerCompleted += workerFinishedApiRouterDataGet;

            worker.RunWorkerAsync();

            //remove all back and forwards keyboard shortcuts
            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();

        }

       /// <summary>
       /// Loads an image from a file name
       /// </summary>
       /// <param name="filename"> the name of the image in the resources folder</param>
       /// <returns>A BitmapImage which can be used within the router list in the UI </returns>
        private BitmapImage loadImage(string filename)
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
            SQLiteConnection connection = new SQLiteConnection
                (Properties.Settings.Default.RoutersDBConnectionString);
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

            SQLiteDataAdapter sqlData = new SQLiteDataAdapter
                (Properties.Settings.Default.allRouters, connection);
            // put the data in the dataset and source it back to the Routers table
            sqlData.Fill(returnedDS);
            
            return returnedDS;
        }
        /// <summary>
        /// Gets the price by calling the Walmart web service for the given item id
        /// </summary>
        /// <param name="itemid">the itemid of the router</param>
        /// <returns>a string representation of the price</returns>
        private Dictionary<string, ArrayList> getApiData()
        {

            Dictionary<string, ArrayList> allRouterResultsDict = 
                new Dictionary<string, ArrayList>();

            foreach (RouterData router in routerDataList)
            {
                string itemID = router.ItemID;
                try
                {
                   
                    ArrayList apiResults = new ArrayList();

                    var client = new RestClient(String.Format(
                       ConfigurationManager.
                    AppSettings["walmartApiEndPointProductURL"].ToString(), itemID));
                    var request = new RestRequest(Method.GET);

                    request.AddHeader("x-rapidapi-host", ConfigurationManager.
                    AppSettings["walmartApiEndPointHeader"].ToString());
                    request.AddHeader("x-rapidapi-key", ConfigurationManager.
                    AppSettings["walmartApiKey"].ToString());

                    IRestResponse response =  client.Execute(request);

                    var results = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>
                        (response.Content);

                    var data = results["data"];

                    string priceString = data["productByProductIdHolidays"]["offerList"]
                        [0]["pricesInfo"]
                        ["prices"]["current"]["price"];
                    string productUrlString = data["productByProductIdHolidays"]
                        ["productAttributes"]["canonicalUrl"];
                    string productRatingString = data["productByProductIdHolidays"]
                        ["productAttributes"]["averageRating"];

                    if (!String.IsNullOrEmpty(priceString) && 
                        !String.IsNullOrEmpty(productUrlString) &&
                        !String.IsNullOrEmpty(productRatingString))
                    {
                        apiResults.Add(priceString);
                        apiResults.Add(productUrlString);
                        apiResults.Add(productRatingString);
                    }

                    allRouterResultsDict.Add(itemID, apiResults);

                    // wait a little bit to not hit request limits.
                    Thread.Sleep(API_SLEEP_MS);

                }
                //catch any web service or network errors
                catch (Exception e)
                {
                    allRouterResultsDict.Add(itemID, null);
                }
            }
            return allRouterResultsDict;
        }

        /// <summary>
        /// Helper method for navigation to a url
        /// </summary>
        /// <param name="sender">object which invoke the event, not used here</param>
        /// <param name="e">arguments for the event of requesting navigation to the 
        /// url</param>
        private void URL_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // always consider the event handled, as it could be a valid URL or still loading.
            e.Handled = true;

            if (e.Uri.ToString().Equals(LOADING_API_DATA))
            {
               
                MessageBox.Show(ConfigurationManager.AppSettings["noApiDataYet"].ToString());
            }
            else if (e.Uri.ToString().Length > 0 && !e.Uri.ToString().Equals(DEFAULT_URL_STR))
            {
              
                try
                {
                    string fullUrl = ConfigurationManager.AppSettings["walmartUrl"].ToString() 
                        + e.Uri.ToString();
                    Process.Start(new ProcessStartInfo(fullUrl));
                }
                catch (System.InvalidOperationException ioe)
                {

                    MessageBox.Show(ConfigurationManager.
                        AppSettings["noUrlErrorMessage"].ToString());
                    Debug.WriteLine(ioe.Message);
                }
      
            }
            else
            {
                MessageBox.Show(ConfigurationManager.AppSettings["noUrlErrorMessage"].ToString());
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
                RouterList.ItemsSource = routerDataList;
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
            List<RouterData> routerDataLocalList = new List<RouterData>(numRows);
            for (int i = 0; i < numRows; i++)
            {
                //get all 5 columns which are used in UI from the data set
                routerDR = routerDS.Tables[ROUTER_TABLE].Rows[i];

                routerDataLocalList.Add(new RouterData
                {
                    Brand = routerDR.ItemArray.GetValue(ROUTER_BRAND).ToString(),
                    ImageData = loadImage(routerDR.ItemArray.GetValue(ROUTER_IMAGE).ToString()),
                    Model = routerDR.ItemArray.GetValue(ROUTER_MODEL).ToString(),
                    Price = LOADING_API_DATA,
                    URL = LOADING_API_DATA,
                    URLName = LOADING_API_DATA,
                    Rating = LOADING_API_DATA,
                    ItemID = routerDR.ItemArray.GetValue(ROUTER_ITEM_ID).ToString(),
                    Frequency = routerDR.ItemArray.GetValue(ROUTER_FREQUENCY).ToString(),
                    Power = routerDR.ItemArray.GetValue(ROUTER_POWER).ToString(),
                    IsHighPower = Convert.ToBoolean(Convert.ToInt32(routerDR.ItemArray.GetValue
                                  (ROUTER_IS_HIGH_POWER).ToString()))

                });
            }

            // set the WPF list to the array of router data objects
            RouterList.ItemsSource = routerDataLocalList;
            routerDataList = routerDataLocalList;
            
        }
        /// <summary>
        /// search from the routerList and find only routers whos brand or model contain the 
        /// search terms
        /// </summary>
        /// <param name="searchTerm">the search terms to look for</param>
        private void searchAndUpdateRouters(string searchTerm)
        {
            List<RouterData> searchResults = new List<RouterData>();
            foreach(RouterData router in routerDataList)
            {
                // Brand or Model contains the search term add it to he result list
                if (router.Brand.ToLower().Contains(searchTerm) || router.Model.ToLower().
                    Contains(searchTerm))
                {
                    searchResults.Add(router);
                }
            }
            // set the WPF list to the results
            RouterList.ItemsSource = searchResults;
        }
        /// <summary>
        /// Event listener for when a column is clicked for sorting
        /// </summary>
        /// <param name="sender">object which the event came from</param>
        /// <param name="e">arguments from the event in this case used to get the correct 
        /// source</param>
        private void Column_Click(object sender, RoutedEventArgs e)
        {
            //clear searches 
            SearchRouters.Clear();
            GridViewColumnHeader columnClicked = e.OriginalSource as GridViewColumnHeader;
            if (columnClicked == null)
                return;

            // get only name of column sorted
            string column = columnClicked.ToString().Split(new string[] { ": " }, 
                StringSplitOptions.None).Last();
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

            switch (lastSorted) {

                case ("Brand"):
                    RouterList.ItemsSource = sortSwitch ? routerDataList.
                            OrderByDescending(obj => obj.Brand).ToList() : 
                            routerDataList.OrderBy(obj => obj.Brand).ToList();
                    break;
                case ("Model"):
                    RouterList.ItemsSource = sortSwitch ? routerDataList.
                        OrderByDescending(obj => obj.Model).ToList() : 
                        routerDataList.OrderBy(obj => obj.Model).ToList();
                    break;
                case ("Current Price"):
                    RouterList.ItemsSource = sortSwitch ? routerDataList.
                        OrderByDescending(obj => Convert.ToDouble(obj.Price)).ToList() :
                        routerDataList.OrderBy(obj => Convert.ToDouble(obj.Price)).ToList();
                    break;
                case ("Rating"):   
                    RouterList.ItemsSource = sortSwitch ? routerDataList.
                        OrderByDescending(obj => Convert.ToDouble(obj.Rating)).ToList() :
                        routerDataList.OrderBy(obj => Convert.ToDouble(obj.Rating)).ToList();
                    break;
                default:
                    break;
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
                {
                    tempColumnClear = tempColumnClear.Split(new string[] { " \u25BC" },
                        StringSplitOptions.None).First();
                }
                else if (columnGridViewClear.Header.ToString().Contains("\u25B2"))
                {
                    tempColumnClear = tempColumnClear.Split(new string[] { " \u25B2" },
                        StringSplitOptions.None).First();
                }
                //reset all columns to orginal state
                columnGridViewClear.Header = tempColumnClear;
                //reset sorting to ascending
                this.sortSwitch = false;
            }
        }
        /// <summary>
        /// Method to assign work to background worker, which will call API for each router in our 
        /// DB
        /// </summary>
        /// <param name="sender"> sender not used here</param>
        /// <param name="e">args not used here</param>
        private void workerGetApiRouterData(object sender, DoWorkEventArgs e)
        {
            allRouterApiResultsDict = getApiData();
        }
        /// <summary>
        /// Once the API calls have been completed, we will update our internal routerlist 
        /// and refresh the items.
        /// </summary>
        /// <param name="sender"> sender not used here</param>
        /// <param name="e">args not used here</param>
        private void workerFinishedApiRouterDataGet(object sender,
                                                   RunWorkerCompletedEventArgs e)
        {
            
            foreach(KeyValuePair<string, ArrayList> item in allRouterApiResultsDict)
            {
                string itemIDDict = item.Key;
                int currentRouterIndex = routerDataList.FindIndex(x => x.ItemID.Equals
                (itemIDDict));
                if(currentRouterIndex >= 0 && item.Value != null)
                {
                    ArrayList apiResultDict = item.Value;
                    routerDataList[currentRouterIndex].Price = apiResultDict[API_PRICE_INDEX].
                        ToString();
                    routerDataList[currentRouterIndex].URL = apiResultDict[API_URL_INDEX].
                        ToString();
                    routerDataList[currentRouterIndex].URLName =
                        routerDataList[currentRouterIndex].Model + " Store Page";
                    routerDataList[currentRouterIndex].Rating = apiResultDict[API_RATING_INDEX].
                        ToString();

                }
                // if for some reason the API get not get data for this router, set some default
                // values
                else
                {
                    routerDataList[currentRouterIndex].Price = DEFAULT_PRICE_STR;
                    routerDataList[currentRouterIndex].URLName = DEFAULT_URL_STR;
                    routerDataList[currentRouterIndex].URL = DEFAULT_URL_STR;
                    routerDataList[currentRouterIndex].Rating = DEFAULT_RATING_STR;
                }
            }

            RouterList.ItemsSource = routerDataList;
            RouterList.Items.Refresh();
           

        }
        /// <summary>
        /// Return our global list of all routers, this is used by coverage class. 
        /// Much faster to store it here than requery the database in the coverage class.
        /// </summary>
        /// <returns>List<RouterData> list of routers </returns>
        public static List<RouterData> getRouterDataList()
        {
            return routerDataList;
        }
       
    }
   
}

