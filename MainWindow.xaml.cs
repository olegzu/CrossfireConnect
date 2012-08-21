using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace CrossfireConnect
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CrossFire.NewData += new CrossFire.StringHandler(CrossFire_NewData);
            CrossFire.Error += new EventHandler(CrossFire_Error);
        }

        void CrossFire_Error(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                textBox2.Text += "\n" + CrossFire.LastError;
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new EventHandler(CrossFire_Error), sender, e);
            }
        }

        void CrossFire_NewData(string data)
        {
            if (Dispatcher.CheckAccess())
            {
                textBox2.Text += "\n" + data;
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new CrossfireConnect.CrossFire.StringHandler(CrossFire_NewData), data);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            CrossFire.Connect();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            CrossFire.GetListContexts();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            CrossFire.GetTools();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            CrossFire.SendCommand("request", "createContext", null, new string[] { "\"url\": \"" + textBox1.Text + "\"" });
        }
    }
}
