﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace WifiRanger
{
    /// <summary>
    /// Interaction logic for Floors.xaml
    /// </summary>
    public partial class Floors : Page
    {
        private DispatcherTimer coverageTimer;
        private double percentCoverageVal = 0.0;
        private double counter = 0.0;
        private static readonly int INTERFERENCE_CONST = 1;
        public Floors()
        {
            InitializeComponent();
            Console.WriteLine("init called");
            Console.WriteLine(Application.Current.Properties["Floors"]);
            Console.WriteLine(this.getRouterFrequency(Application.Current.Properties["SelectedRouter"].ToString()));
            double meters = this.calculateDistance(-65,this.getRouterFrequency(Application.Current.Properties["SelectedRouter"].ToString()));
            Console.WriteLine(meters);
            Console.WriteLine(this.calulateCoverage(meters, Convert.ToDouble(Application.Current.Properties["SQ_Feet"].ToString())));
            percentCoverageVal = this.calulateCoverage(meters, Convert.ToDouble(Application.Current.Properties["SQ_Feet"].ToString()));
            coverageTimer = new DispatcherTimer();
            coverageTimer.Interval = new TimeSpan(0, 0, 0,0,10);
            coverageTimer.Tick += CoverageTimer_Tick;





        }

        private void CoverageTimer_Tick(object sender, EventArgs e)
        {
            counter += 1;
            PercentCoverage.Content = counter + " %";
            if (counter >= percentCoverageVal)
            {
                coverageTimer.Stop();
            }
            else if (percentCoverageVal < 30)
                PercentCoverage.Foreground = Brushes.Red;
            else if (percentCoverageVal > 30 && percentCoverageVal < 70)
                PercentCoverage.Foreground = Brushes.Orange;
            else if (percentCoverageVal > 70)
                PercentCoverage.Foreground = Brushes.PaleGreen;
            
        }


        private double calculateDistance(double levelInDb, double freqInMHz)
        {
            double exp = (27.55 - INTERFERENCE_CONST - (20 * Math.Log10(freqInMHz)) + Math.Abs(levelInDb)) / 20.0;
            // times 2 to get a better reading, after real world tests. 
            return Math.Pow(10.0, exp);
        }
        private double calulateCoverage(double distance,double area)
        {
            //sqft to sqmeters
            //area = area * .3048;
            double length = (Math.Sqrt(area/70)) * 10;
            double width = (Math.Sqrt(area/70)) * 7;

            double lengthCoverage = distance / length;
            double widthCoverage = distance / width;

            double hypoCoverage = Math.Sqrt(Math.Pow(length,2) + Math.Pow(width,2));
            double cov = (distance / hypoCoverage) * 100;
            double coverage = (lengthCoverage/2 + widthCoverage/2) * 100;
            Console.WriteLine(coverage + " cov val");
            if (cov > 100)
                return 100;
            else
                return cov;


        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            coverageTimer.Start();
        }

        private int getRouterFrequency(String model)
        {
            SqlConnection connection = new SqlConnection(Properties.Settings.Default.RoutersDBConnectionString);

            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
               
                Console.WriteLine("Can not open connection! " + ex.ToString());
            }

            SqlCommand sql = new SqlCommand(ConfigurationManager.AppSettings["getFrequency"].ToString(), connection);
            sql.Parameters.AddWithValue("@model", model);
            int frequency = (int) sql.ExecuteScalar();
            return frequency;
        }
    }
}
