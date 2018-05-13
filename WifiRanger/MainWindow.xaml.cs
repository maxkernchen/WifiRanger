using System.Windows;
using System.Windows.Input;

namespace WifiRanger
{
    /// <summary>
    /// This class just holds the Window class which is needed for starting up the applications
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Construtor which sends us to the Router Page on start up
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _NavigationFrame.Navigate(new Routers());
            //remove back and forward short cut buttons
            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();

        }

    }
}