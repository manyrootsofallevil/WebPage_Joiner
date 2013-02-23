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

namespace HTMLJoiner
{
    /// <summary>
    /// Interaction logic for ImproveMe.xaml
    /// </summary>
    public partial class ImproveMe : Window
    {
        string domain;

        public ImproveMe(string domain)
        {
            InitializeComponent();

            this.domain = domain;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AddDomainToFile(Id.Text.Trim(), domain);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
