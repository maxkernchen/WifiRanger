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
    /// Interaction logic for Routers.xaml
    /// </summary>
    public partial class Routers : Page
    {
        public Routers()
        {
            InitializeComponent();
            RouterList.ItemsSource = new RouterData[]
       {
            new RouterData{Title="Router1", ImageData=LoadImage("wireless-router.jpg")},
             new RouterData{Title="Router2", ImageData=LoadImage("wireless-router.jpg")},
              new RouterData{Title="Router3", ImageData=LoadImage("wireless-router.jpg")},
               new RouterData{Title="Router4", ImageData=LoadImage("wireless-router.jpg")},
                new RouterData{Title="Router5", ImageData=LoadImage("wireless-router.jpg")}

       };
            
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
            Console.WriteLine(routerData.Title);
        }
    }
}

