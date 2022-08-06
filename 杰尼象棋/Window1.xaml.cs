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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace JNG音乐
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public bool next = false;
        public DispatcherTimer timer1 = new DispatcherTimer();
        public Window1()
        {
            InitializeComponent();

            /*Task.Run(async () =>
            {
                while (!next)
                {
                    await Task.Delay(500);
                }
                await Task.Delay(3000);
                this.Close();
            });*/
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
