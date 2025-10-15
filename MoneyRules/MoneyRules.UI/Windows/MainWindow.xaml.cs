using MoneyRules.Infrastructure.Persistence;
using MoneyRules.UI.Windows;
using MoneyRules.Application.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace MoneyRules.UI
{
    public partial class MainWindow : Window
    {
        private readonly IAdviceService _adviceService;

        public MainWindow()
        {
            InitializeComponent();
            _adviceService = new AdviceService();
            LoadAdvice();
        }

        private void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddTransactionWindow
            {
                Owner = this
            };

            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                MessageBox.Show("Транзакція успішно додана!");
                // refresh advice when a new transaction is added
                LoadAdvice();
            }
        }
        private void LoadAdvice()
        {
            try
            {
                using var db = new AppDbContext();
                var transactions = db.Transactions
                    .OrderByDescending(t => t.Date)
                    .ToList();

                var tips = _adviceService.GetAdvice(transactions);
                AdviceList.ItemsSource = tips.Select(t => new TextBlock { Text = t, TextWrapping = TextWrapping.Wrap });
            }
            catch (System.Exception ex)
            {
                AdviceList.ItemsSource = new[] { new TextBlock { Text = "Не вдалося отримати поради: " + ex.Message } };
            }
        }

        private void RefreshAdviceButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAdvice();
        }
    }
}
