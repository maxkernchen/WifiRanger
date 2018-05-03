using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WifiRanger
{
    /// <summary>
    /// MainPage class which holds the inputs for calculating the router's range
    /// </summary>
    public partial class MainPage : Page
    {
     
       // how many times the textbox flashes red for invalid input
        private static readonly int FLASH_COUNT = 4;
        //counter for counting number of flashes for invalid input
        private int flashCounter = 0;
        //dispatcher counter to flash the red in the textbox without locking the UI.
        private DispatcherTimer flashAreaText;

        /// <summary>
        /// Constuctor which sets up inital textboxes and if buttons or combo boxes are enabled
        /// </summary>
       public MainPage() {
            InitializeComponent();
           
            //remove back and forward shortcuts
            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();
            //the area textbox placeholder text should be gray
            Area_TextBox.Foreground = new SolidColorBrush(Colors.Gray);
            SQ_Feet_Radio.IsChecked = true;
            SQ_Meter_Radio.IsChecked = false;
            Calculate_Button.IsEnabled = false;
            //create the dispatchertimer to run every 200 milliseconds
            flashAreaText = new DispatcherTimer();
            flashAreaText.Interval = new TimeSpan(0, 0, 0, 0, 200);
            flashAreaText.Tick += CoverageTimer_Tick;
        }
        /// <summary>
        /// Event listener for the Calculate button click
        /// </summary>
        /// <param name="sender">the object which sent the event, not used in this case</param>
        /// <param name="e">arguments from the event, not used in this case</param>
        private  void Calculate_Button_Click(object sender, RoutedEventArgs e)
        {
           
            int area = -1;
            //try to parse the string into an int 
            try
            {
                area = int.Parse(Area_TextBox.Text);
            }
            catch(System.FormatException fe)
            {
                Debug.WriteLine(fe.Message);
            }
            //check that there is a floor selected, the area is between 1 and 10,000, and the routerlocation is also selected
            if (Floors.SelectedItem != null && (area >= 1 && area <= 10000))
            {
                //stored the area in the application properties
                Application.Current.Properties["Area"] = Area_TextBox.Text;
                //store what the unit is
                if (SQ_Feet_Radio.IsChecked.Value)
                    Application.Current.Properties["Unit"] = "Feet";
                else
                    Application.Current.Properties["Unit"] = "Meter";
                //get the floor number only, spilt the : for only the number
                int floor_num = int.Parse(Floors.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last());

                ///minus 1 since a one story house does not need to factor in height
                Application.Current.Properties["Floors"] = floor_num -1;
                //stored the router location
                Application.Current.Properties["RouterLocation"] = RouterLocation.SelectedIndex;
                // go to the coverage page
                this.NavigationService.Navigate(new Uri("Coverage.xaml", UriKind.Relative));

            }
            //flash the area text if the inputs are not correct
            else
            {
                flashCounter = 0;
                flashAreaText.Start();
                //call the validation manually that is set on the area box
                Area_TextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }

        }
        /// <summary>
        /// When the floors combobox is closed, check that if inputs are valid and enable the calculate button
        /// </summary>
        /// <param name="sender">where the event came from, not used in this case</param>
        /// <param name="e">arguments from the event, not used in this case</param>
        private void Floors_DropDownClosed(object sender, EventArgs e)
        {
            //check that there is at least one item selected 
            if (Floors.SelectedItem != null && RouterLocation.SelectedItem != null)
                Calculate_Button.IsEnabled = true;
        }
        
        /// <summary>
        /// If the area textbox was clicked on, then clear the placeholder
        /// </summary>
        /// <param name="sender">where the event came from, not used in this case</param>
        /// <param name="e">arguments for the event, not used in this case</param>
        private void SQ_Feet_GotFocus(object sender, RoutedEventArgs e)
        {                
                Area_TextBox.Clear();
                Area_TextBox.Foreground = new SolidColorBrush(Colors.Black);                      
        }
        /// <summary>
        /// Checks if the router location drop down was closed, 
        /// then enabled the calculate button if the floors drop down also contains a selection
        /// </summary>
        /// <param name="sender">where the event came from, not used in this case</param>
        /// <param name="e">arguments for the event, not used in this case</param>
        private void RouterLocation_DropDownClosed(object sender, EventArgs e)
        {
            if(Floors.SelectedItem != null && RouterLocation.SelectedItem !=null)
            {
                Calculate_Button.IsEnabled = true;
            }
        }
        /// <summary>
        /// if the meters radio button is clicked set the placeholder back
        /// </summary>
        /// <param name="sender">where the event came from, not used in this case</param>
        /// <param name="e">arguments for the event, not used in this case</param>
        private void SQ_Meter_Radio_Checked(object sender, RoutedEventArgs e)
        {
            Area_TextBox.Foreground = new SolidColorBrush(Colors.Gray);
            this.Area_TextBox.Text = "Square Meters";
        }
        /// <summary>
        /// if the square feet radio button is clicked set the placeholder back
        /// </summary>
        /// <param name="sender">where the event came from, not used in this case</param>
        /// <param name="e">arguments for the event, not used in this case</param>
        private void SQ_Feet_Radio_Checked(object sender, RoutedEventArgs e)
        {
            Area_TextBox.Foreground = new SolidColorBrush(Colors.Gray);
            this.Area_TextBox.Text = "Square Feet";
        }
        /// <summary>
        /// A tick method for the dispatcher timer which flashes the area textbox red 
        /// when there is invalid input
        /// </summary>
        /// <param name="sender">where the event came from, not used in this case</param>
        /// <param name="e">arguments for the event, not used in this case</param>
        private void CoverageTimer_Tick(object sender, EventArgs e)
        {
            if (flashCounter > FLASH_COUNT)
                flashAreaText.Stop();
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
