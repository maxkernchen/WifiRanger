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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private String firstText;
        private TextBox text;
        public MainWindow()
        {
            InitializeComponent();
            text = new TextBox();
            text.Name = "text1";
            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void Caluclate_Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(TextInput.Text);
        }
    }
}
