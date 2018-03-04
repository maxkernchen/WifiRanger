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
        private static readonly int routerName = 1;
        // location of Router table in router database
        private static readonly int routerTable = 0;

        public Routers()
        {
            InitializeComponent();
            DataSet routerDS = this.getRouterData();
            DataRow routerDR;
            numRows = routerDS.Tables[0].Rows.Count;
            RouterData[] routerDataArray = new RouterData[numRows];
            for(int i = 0; i < numRows; i++)
            {
                routerDR = routerDS.Tables[routerTable].Rows[i];
                routerDataArray[i] = new RouterData
                {
                    Name = routerDR.ItemArray.GetValue(routerName).ToString(),
                    ImageData = LoadImage("wireless-router.jpg")
                };
            }


            RouterList.ItemsSource = routerDataArray;
      

          
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
                Console.WriteLine("Can not open connection ! " + ex.ToString());
            }

            SqlDataAdapter sqlData = new SqlDataAdapter(Properties.Settings.Default.allRouters, connection);
            DataSet routerDataSet = new DataSet();
            // put the data in the dataset and source it back to the Routers table
            sqlData.Fill(routerDataSet, "Routers");

            return routerDataSet;
        }
    }

    
}

