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
using PhysioDetectors;
using System.Threading;
using System.Globalization;
using System.Resources;

namespace Jeran
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IDetector detector;
        Thread connectdev;
        DilemmaRespondent respondent;

        public bool CanStart
        {
            get { return (bool)GetValue(CanStartProperty); }
            set { SetValue(CanStartProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanStart.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanStartProperty =
            DependencyProperty.Register("CanStart", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public MainWindow()
        {
            try
            {

                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(Properties.Settings.Default.CultureInfo);
            }
            catch
            {
                Console.WriteLine("Culture info trouble, ue en-US as default.");
            }

            InitializeComponent();
            if (Properties.Settings.Default.ShowConsole)
                ConsoleManager.Show();
            CanStart = true;
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
            //respondent = new jRespondent(Properties.Resources.DillemFolder);
            //Testing test = new Testing(respondent, Properties.Resources.DillemFolder);
            //test.Show();
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void newtest_Click(object sender, RoutedEventArgs e)
        {
            respondent = new DilemmaRespondent();
            Testing test = new Testing(respondent, Properties.Resources.DillemFolder);
            test.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            connectdev = new Thread(() =>
                {
                    detector = new Zephyr();
                    detector.OnData += detector_OnData;
                    do
                    {
                        Thread.Sleep(1000);
                        if (string.IsNullOrEmpty(Properties.Settings.Default.PrefPort))
                            detector.ConnectDevice();
                        else
                            detector.ConnectDevice(Properties.Settings.Default.PrefPort);
                        
                    } while (!detector.CheckConnection());
                });
            connectdev.Start();
        }

        byte _hxmcount = 0;
        void detector_OnData(object data)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate { 
                CanStart = true;
                hxmstatus.Text = Properties.Resources.hxmReady;
            }));

            HXMdata chunk = (HXMdata)data;
            if (respondent != null && _hxmcount != chunk.HeartBeatNum)
            {
                byte diff = (byte)(chunk.HeartBeatNum - _hxmcount);

                for (int i = 0; i < diff; i++)
                {
                    int RR = chunk.RR(diff - i);
                    if (RR != 0)
                    {
                        Console.WriteLine("{0} - {1} ms", diff - i, RR);
                        respondent.RR.Add(RR);
                    }
                }
                _hxmcount = chunk.HeartBeatNum;

                
                respondent.HeartRate = chunk.HeartRate;
                respondent.Charge = chunk.BatteryCharge;
            }
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (connectdev != null && connectdev.IsAlive)
                connectdev.Abort();
            detector.Dispose();
        }

        private void viewdb_Click(object sender, RoutedEventArgs e)
        {
            DB dbview = new DB();
            dbview.ShowDialog();
        }
    }
}
