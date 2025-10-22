using System.Windows;
using System.Windows.Controls;

namespace MoneyRules.UI.Windows
{
    public partial class FileUploadPage : UserControl
    {
        public FileUploadPage()
        {
            InitializeComponent();
        }

        private void UploadFile_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Функція завантаження ще не реалізована.";
        }
    }
}
