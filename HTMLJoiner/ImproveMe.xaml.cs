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

        public static RoutedCommand SaveCommand = new RoutedCommand();

        private Validator validator = new Validator();

        int validationErrors = 0;

        public ImproveMe(string domain)
        {
            InitializeComponent();
            this.domain = domain;
            this.DataContext = validator;
        }

        //TODO: Improve Validation around here
        private void Update_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(TagName.Text.Trim()) &&
                !string.IsNullOrEmpty(TagAttribute.Text.Trim()))
            {
                MainWindow.AddDomainToFile(domain, TagName.Text.Trim(), TagAttribute.Text.Trim(), Id.Text.Trim());
                this.DialogResult = true;
                this.Close();

            }
            else if (string.IsNullOrEmpty(TagName.Text.Trim()) &&
                string.IsNullOrEmpty(TagAttribute.Text.Trim()))
            {

                MainWindow.AddDomainToFile(Id.Text.Trim(), domain);
                this.DialogResult = true;
                this.Close();

            }
            else
            {
                MessageBox.Show("In order to save the details, ensure that you fill all fields or just the id field");
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = validationErrors == 0;
            e.Handled = true;
        }

        private void Id_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                validationErrors++;
            }
            else
            {
                validationErrors--;
            }
        }

        private void TagName_Error(object sender, ValidationErrorEventArgs e)
        {

        }
    }
}
