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
using System.Drawing;

namespace WifiRanger
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
       private int FloorsVal = 0;
       private bool entered_SQFoot_Text;
      
       public MainPage() {
            InitializeComponent();
            Picked_Floor.IsEnabled = false;
            entered_SQFoot_Text = false;
            SQ_Feet.Foreground = new SolidColorBrush(Colors.Gray);
            Calculate_Button.IsEnabled = false;
      
         
       
       }

        private void Calculate_Button_Click(object sender, RoutedEventArgs e)
        {

            if(Picked_Floor.SelectedItem != null)
            {
                FloorsVal = int.Parse(Picked_Floor.Text.ToString());
                
                Application.Current.Properties["Floors"] = FloorsVal;
                this.NavigationService.Navigate(new Uri("Floors.xaml", UriKind.Relative));
            }
            else
            {
               

            }

        }

        private void Floors_DropDownClosed(object sender, EventArgs e)
        {
            {
                //check that there is at least one item selected 
                if (Floors.SelectedItem != null)
                {
                    Picked_Floor.Items.Clear();
                    Picked_Floor.IsEnabled = true;
                    Calculate_Button.IsEnabled = false;
                    // just get the int value 
                    int floor_num = int.Parse(Floors.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last());
                    for(int i = 1; i <= floor_num; i++)
                     {
                         Picked_Floor.Items.Add(i);
                     }
                     
                }


            }
        }

        private void SQ_Feet_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!entered_SQFoot_Text) {
                SQ_Feet.Clear();
                SQ_Feet.Foreground = new SolidColorBrush(Colors.Black);
                entered_SQFoot_Text = true;
            }              
        }

        private void Picked_Floor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Calculate_Button.IsEnabled = true;
        }
    }
}
