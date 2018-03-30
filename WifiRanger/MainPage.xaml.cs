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
      // private int FloorsVal = 0;
      // private bool entered_SQFoot_Text;
        private bool Floor_Entered = false;
      
       public MainPage() {
            InitializeComponent();
            //Picked_Floor.IsEnabled = false;
            //entered_SQFoot_Text = false;
            //RouterLocation.IsEnabled = false;
            Area_TextBox.Foreground = new SolidColorBrush(Colors.Gray);
            SQ_Feet_Radio.IsChecked = true;
            SQ_Meter_Radio.IsChecked = false;
            Calculate_Button.IsEnabled = false;
      
         
       
       }

        private void Calculate_Button_Click(object sender, RoutedEventArgs e)
        {

            if(Floors.SelectedItem != null)
            {
               // FloorsVal = int.Parse(Picked_Floor.Text.ToString());
                
                //Application.Current.Properties["Floors"] = FloorsVal;
                Application.Current.Properties["Area"] = Area_TextBox.Text;
                this.NavigationService.Navigate(new Uri("Floors.xaml", UriKind.Relative));
            }
            else
            {
               

            }

        }

        private void Floors_DropDownClosed(object sender, EventArgs e)
        {

            //check that there is at least one item selected 
            if (Floors.SelectedItem != null && RouterLocation.SelectedItem !=null && Area_TextBox.Text != "")
            {
                // Picked_Floor.Items.Clear();
                // Picked_Floor.IsEnabled = true;
            
               
                    Calculate_Button.IsEnabled = true;

                // just get the int value 
                int floor_num = int.Parse(Floors.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last());
                Application.Current.Properties["Floors"] = floor_num;
                // for(int i = 1; i <= floor_num; i++)
                // {
                //Picked_Floor.Items.Add(i);
                //}

            }


        }
        

        private void SQ_Feet_GotFocus(object sender, RoutedEventArgs e)
        {
            
                Area_TextBox.Clear();
                Area_TextBox.Foreground = new SolidColorBrush(Colors.Black);
              
                       
        }

        private void Picked_Floor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Calculate_Button.IsEnabled = true;
            //RouterLocation.IsEnabled = true;
        }

        private void RouterLocation_DropDownClosed(object sender, EventArgs e)
        {
            if(Floors.SelectedItem != null && RouterLocation.SelectedItem !=null && Area_TextBox.Text != "")
            {
                Calculate_Button.IsEnabled = true;
            }
        }

        private void SQ_Meter_Radio_Checked(object sender, RoutedEventArgs e)
        {
            Area_TextBox.Foreground = new SolidColorBrush(Colors.Gray);
            this.Area_TextBox.Text = "Square Meters";
        }

        private void SQ_Feet_Radio_Checked(object sender, RoutedEventArgs e)
        {
            Area_TextBox.Foreground = new SolidColorBrush(Colors.Gray);
            this.Area_TextBox.Text = "Square Feet";
        }
    }
}
