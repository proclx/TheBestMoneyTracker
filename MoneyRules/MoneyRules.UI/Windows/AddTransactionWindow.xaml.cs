using System.Windows;
using MoneyRules.UI.ViewModels;

namespace MoneyRules.UI.Windows
{
    public partial class AddTransactionWindow : Window
    {
        // Конструктор для DI/MVVM
        public AddTransactionWindow(AddTransactionViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        // Конструктор без параметрів (для Design Time або якщо використовується нечистий DI)
        public AddTransactionWindow()
        {
            InitializeComponent();
        }

        // Логіка OkButton_Click, CreatedTransaction та конструктор дублювання ВИДАЛЕНІ

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}