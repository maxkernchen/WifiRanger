using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WifiRanger
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
      // private int FloorsVal = 0;
      // private bool entered_SQFoot_Text;
      
        private static readonly int FLASH_COUNT = 4;
   
        private int flashCounter = 0;
     
        private DispatcherTimer flashAreaText;


       public MainPage() {
            InitializeComponent();
            //Picked_Floor.IsEnabled = false;
            //entered_SQFoot_Text = false;
            //RouterLocation.IsEnabled = false;
            Area_TextBox.Foreground = new SolidColorBrush(Colors.Gray);
            SQ_Feet_Radio.IsChecked = true;
            SQ_Meter_Radio.IsChecked = false;
            Calculate_Button.IsEnabled = false;
            flashAreaText = new DispatcherTimer();
            flashAreaText.Interval = new TimeSpan(0, 0, 0, 0, 200);
            flashAreaText.Tick += CoverageTimer_Tick;



        }

        private  void Calculate_Button_Click(object sender, RoutedEventArgs e)
        {
           
            int area = -1;
            try
            {
                area = int.Parse(Area_TextBox.Text);
            }
            catch(System.FormatException fe)
            {
                Console.WriteLine(fe.Message);
            }
            if (Floors.SelectedItem != null && (area >= 1 && area <= 10000))
            {
               // FloorsVal = int.Parse(Picked_Floor.Text.ToString());
                
                //Application.Current.Properties["Floors"] = FloorsVal;
                Application.Current.Properties["Area"] = Area_TextBox.Text;
                if (SQ_Feet_Radio.IsChecked.Value)
                    Application.Current.Properties["Unit"] = "Feet";
                else
                    Application.Current.Properties["Unit"] = "Meter";

                int floor_num = int.Parse(Floors.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last());
                Application.Current.Properties["Floors"] = floor_num;
                Application.Current.Properties["RouterLocation"] = RouterLocation.SelectedIndex;
                this.NavigationService.Navigate(new Uri("Floors.xaml", UriKind.Relative));

            }
            else
            {
                flashCounter = 0;

                flashAreaText.Start();

              //  SpinWait.SpinUntil(() => done == true, 10000);
                Area_TextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }

        }

        private void Floors_DropDownClosed(object sender, EventArgs e)
        {

            //check that there is at least one item selected 
            if (Floors.SelectedItem != null && RouterLocation.SelectedItem !=null)
            {
                // Picked_Floor.Items.Clear();
                // Picked_Floor.IsEnabled = true;
            
               
                    Calculate_Button.IsEnabled = true;

                // just get the int value 
               

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
            if(Floors.SelectedItem != null && RouterLocation.SelectedItem !=null)
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
        private void CoverageTimer_Tick(object sender, EventArgs e)
        {
            if(flashCounter > FLASH_COUNT)
            {
                flashAreaText.Stop();

            }
            else
            {
                //alternate between colors
                if (flashCounter % 2 == 0)
                    Area_TextBox.Background = new SolidColorBrush(Colors.White);
                else
                    Area_TextBox.Background = new SolidColorBrush(Colors.Red);
                flashCounter++;
            }

         
        }
    }
}
