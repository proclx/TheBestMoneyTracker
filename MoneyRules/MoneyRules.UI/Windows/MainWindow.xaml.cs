using System.Windows;
using MoneyRules.Application.Services;
using MoneyRules.UI.Windows;
using System.Windows.Controls;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using MoneyRules.Infrastructure.Persistence;
// ...existing using directives...

namespace MoneyRules.UI
{
    public partial class MainWindow : Window
    {
        private readonly ITransactionService _transactionService;
        private readonly IAuthService _authService;
        private readonly IAdviceService _adviceService;
        private readonly IUserProfileService _profileService;
        private User _currentUser;

        public MainWindow(ITransactionService transactionService, IAuthService authService, IUserProfileService profileService)
        {
            InitializeComponent();
            _transactionService = transactionService;
            _authService = authService;
            _adviceService = new AdviceService();
            _profileService = profileService;

            LoadAdvice();
            LoadUserProfile();

            // redraw when canvas size changes so labels stay readable
            if (FindName("ChartCanvas") is Canvas _chart)
                _chart.SizeChanged += (s, e) => DrawChartForSelectedYear();
        }

        private void OpenHistory_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current.Properties["CurrentUser"] is not User currentUser)
            {
                MessageBox.Show("Користувач не знайдений. Увійдіть знову.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var historyWindow = new TransactionHistoryWindow(_transactionService, currentUser.UserId);
            historyWindow.ShowDialog();
        }

        private void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddTransactionWindow { Owner = this };
            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                MessageBox.Show("Транзакція успішно додана!");
                LoadAdvice();
            }
        }

        private void LoadAdvice()
        {
            try
            {
                using var db = new AppDbContext();
                var query = db.Transactions.AsQueryable();
                if (_currentUser != null)
                    query = query.Where(t => t.UserId == _currentUser.UserId);

                var transactions = query.OrderByDescending(t => t.Date).ToList();

                var tips = _adviceService.GetAdvice(transactions);
                var adviceList = FindName("AdviceList") as ItemsControl;
                if (adviceList != null)
                    adviceList.ItemsSource = tips.Select(t => new TextBlock { Text = t, TextWrapping = TextWrapping.Wrap });
            }
            catch (System.Exception ex)
            {
                var adviceList = FindName("AdviceList") as ItemsControl;
                if (adviceList != null)
                    adviceList.ItemsSource = new[] { new TextBlock { Text = "Не вдалося отримати поради: " + ex.Message } };
            }
        }

        private void RefreshAdviceButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAdvice();
        }

        private void LoadUserProfile()
        {
            _currentUser = System.Windows.Application.Current.Properties["CurrentUser"] as User;
            if (_currentUser == null)
            {
                MessageBox.Show("No user logged in.");
                return;
            }

            var txtName = FindName("TxtName") as TextBox;
            var txtEmail = FindName("TxtEmail") as TextBox;
            if (txtName != null) txtName.Text = _currentUser.Name;
            if (txtEmail != null) txtEmail.Text = _currentUser.Email;

            if (_currentUser.ProfilePhoto != null && _currentUser.ProfilePhoto.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(_currentUser.ProfilePhoto);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    var profileImg = FindName("ProfileImage") as System.Windows.Controls.Image;
                    if (profileImg != null) profileImg.Source = bitmap;
                }
                catch
                {
                    var profileImg = FindName("ProfileImage") as System.Windows.Controls.Image;
                    if (profileImg != null) profileImg.Source = null;
                }
            }

            if (_currentUser.Settings != null)
            {
                var cmbCurrency = FindName("CmbCurrency") as ComboBox;
                if (cmbCurrency != null)
                {
                    foreach (ComboBoxItem item in cmbCurrency.Items)
                    {
                        if (item.Content.ToString() == _currentUser.Settings.Currency)
                        {
                            cmbCurrency.SelectedItem = item;
                            break;
                        }
                    }
                }

                var chkNotifications = FindName("ChkNotifications") as CheckBox;
                if (chkNotifications != null) chkNotifications.IsChecked = _currentUser.Settings.NotificationEnabled;
            }

            // Populate chart years
            PopulateChartYears();
            DrawChartForSelectedYear();
        }

        private void PopulateChartYears()
        {
            try
            {
                var cmb = FindName("CmbChartYear") as ComboBox;
                cmb?.Items.Clear();
                using var db = new AppDbContext();
                var years = db.Transactions
                    .Where(t => t.UserId == _currentUser.UserId)
                    .Select(t => t.Date.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToList();

                if (cmb != null)
                {
                    foreach (var y in years)
                    {
                        cmb.Items.Add(new ComboBoxItem { Content = y.ToString(), Tag = y });
                    }

                    if (cmb.Items.Count > 0)
                        cmb.SelectedIndex = 0;
                }
            }
            catch
            {
                // ignore silently
            }
        }

        private void BtnRefreshChart_Click(object sender, RoutedEventArgs e)
        {
            DrawChartForSelectedYear();
        }

        private void CmbChartYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DrawChartForSelectedYear();
        }

        private void DrawChartForSelectedYear()
        {
            var chart = FindName("ChartCanvas") as Canvas;
            var txtStatus = FindName("TxtChartStatus") as TextBlock;
            var cmb = FindName("CmbChartYear") as ComboBox;

            if (chart == null)
                return; // nothing to draw into

            chart.Children.Clear();

            if (_currentUser == null)
            {
                if (txtStatus != null) txtStatus.Text = "Користувач не завантажений.";
                return;
            }

            int year;
            if (cmb != null && cmb.SelectedItem is ComboBoxItem sel && sel.Tag is int y)
                year = y;
            else
                year = DateTime.Now.Year;

            using var db = new AppDbContext();
            var months = Enumerable.Range(1, 12).Select(m => new
            {
                Month = m,
                Income = db.Transactions.Where(t => t.UserId == _currentUser.UserId && t.Date.Year == year && t.Date.Month == m && t.Type.ToString().ToLower().Contains("income")).Sum(t => (decimal?)t.Amount) ?? 0m,
                Expense = db.Transactions.Where(t => t.UserId == _currentUser.UserId && t.Date.Year == year && t.Date.Month == m && t.Type.ToString().ToLower().Contains("expense")).Sum(t => (decimal?)t.Amount) ?? 0m
            }).ToList();

            var maxVal = Math.Max((double)months.Max(m => m.Income), (double)months.Max(m => m.Expense));
            if (maxVal < 1) maxVal = 1;

            double canvasW = chart.ActualWidth > 0 ? chart.ActualWidth : chart.Width;
            double canvasH = chart.ActualHeight > 0 ? chart.ActualHeight : chart.Height;
            double barWidth = (canvasW - 40) / 12.0;

            for (int i = 0; i < 12; i++)
            {
                var m = months[i];
                double x = 20 + i * barWidth;

                // Income bar (green)
                double hInc = (double)m.Income / maxVal * (canvasH - 60);
                var rectInc = new System.Windows.Shapes.Rectangle
                {
                    Width = barWidth * 0.4,
                    Height = Math.Max(1, hInc),
                    Fill = System.Windows.Media.Brushes.Green,
                    Stroke = System.Windows.Media.Brushes.Black
                };
                Canvas.SetLeft(rectInc, x + barWidth * 0.05);
                Canvas.SetTop(rectInc, canvasH - 30 - rectInc.Height);
                chart.Children.Add(rectInc);

                // Income label (bordered) - measure to center
                var incText = ((decimal)m.Income).ToString("N0");
                var incLabel = new TextBlock
                {
                    Text = incText,
                    FontSize = 11,
                    FontWeight = System.Windows.FontWeights.SemiBold,
                    Foreground = System.Windows.Media.Brushes.White
                };
                var incBorder = new System.Windows.Controls.Border
                {
                    Background = System.Windows.Media.Brushes.DarkGreen,
                    CornerRadius = new CornerRadius(4),
                    Child = incLabel,
                    Padding = new Thickness(6, 2, 6, 2),
                    Opacity = m.Income == 0 ? 0.7 : 1
                };
                incBorder.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                double incW = incBorder.DesiredSize.Width;
                double incLeft = x + barWidth * 0.05 + (barWidth * 0.4 - incW) / 2;
                Canvas.SetLeft(incBorder, incLeft);
                Canvas.SetTop(incBorder, Math.Max(4, canvasH - 36 - rectInc.Height - incBorder.DesiredSize.Height));
                chart.Children.Add(incBorder);

                // Expense bar (red)
                double hExp = (double)m.Expense / maxVal * (canvasH - 60);
                var rectExp = new System.Windows.Shapes.Rectangle
                {
                    Width = barWidth * 0.4,
                    Height = Math.Max(1, hExp),
                    Fill = System.Windows.Media.Brushes.Red,
                    Stroke = System.Windows.Media.Brushes.Black
                };
                Canvas.SetLeft(rectExp, x + barWidth * 0.55);
                Canvas.SetTop(rectExp, canvasH - 30 - rectExp.Height);
                chart.Children.Add(rectExp);

                // Expense label (bordered) - measure to center
                var expText = ((decimal)m.Expense).ToString("N0");
                var expLabel = new TextBlock
                {
                    Text = expText,
                    FontSize = 11,
                    FontWeight = System.Windows.FontWeights.SemiBold,
                    Foreground = System.Windows.Media.Brushes.White
                };
                var expBorder = new System.Windows.Controls.Border
                {
                    Background = System.Windows.Media.Brushes.DarkRed,
                    CornerRadius = new CornerRadius(4),
                    Child = expLabel,
                    Padding = new Thickness(6, 2, 6, 2),
                    Opacity = m.Expense == 0 ? 0.7 : 1
                };
                expBorder.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                double expW = expBorder.DesiredSize.Width;
                double expLeft = x + barWidth * 0.55 + (barWidth * 0.4 - expW) / 2;
                Canvas.SetLeft(expBorder, expLeft);
                Canvas.SetTop(expBorder, Math.Max(4, canvasH - 36 - rectExp.Height - expBorder.DesiredSize.Height));
                chart.Children.Add(expBorder);

                // Month label (below bars)
                var monthLbl = new TextBlock { Text = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i + 1), FontSize = 10 };
                Canvas.SetLeft(monthLbl, x + barWidth * 0.1);
                Canvas.SetTop(monthLbl, canvasH - 12);
                chart.Children.Add(monthLbl);
            }

            if (txtStatus != null) txtStatus.Text = $"Дохід/Витрати за {year}";
        }

        private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Image Files|*.jpg;*.png;*.jpeg" };
            if (dlg.ShowDialog() == true)
            {
                byte[] imageData = File.ReadAllBytes(dlg.FileName);

                // Використовуємо сервіс для зміни фото
                _profileService.ChangeProfilePhoto(_currentUser, imageData);

                using var ms = new MemoryStream(imageData);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                var profileImg = FindName("ProfileImage") as System.Windows.Controls.Image;
                if (profileImg != null) profileImg.Source = bitmap;
            }
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User not loaded.");
                return;
            }

            var txtName2 = FindName("TxtName") as TextBox;
            var txtEmail2 = FindName("TxtEmail") as TextBox;
            if (txtName2 != null) _currentUser.Name = txtName2.Text;
            if (txtEmail2 != null) _currentUser.Email = txtEmail2.Text;

            var cmbCurrency2 = FindName("CmbCurrency") as ComboBox;
            var selectedCurrency = (cmbCurrency2?.SelectedItem as ComboBoxItem)?.Content.ToString()
                        ?? _currentUser.Settings?.Currency
                        ?? "UAH";

            if (_currentUser.Settings == null)
                _currentUser.Settings = new Settings { UserId = _currentUser.UserId };

            _currentUser.Settings.Currency = selectedCurrency;
            var chkNotifications2 = FindName("ChkNotifications") as CheckBox;
            _currentUser.Settings.NotificationEnabled = chkNotifications2?.IsChecked ?? false;

            // Використовуємо сервіс для збереження змін
            _profileService.UpdateUser(_currentUser);

            MessageBox.Show("Profile updated successfully.");
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Ви впевнені, що хочете вийти з акаунту?",
                                         "Підтвердження виходу",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Properties["CurrentUser"] = null;

                var welcomeWindow = new WelcomeWindow(_authService, _transactionService, _profileService);
                welcomeWindow.Show();

                this.Close();
            }
        }

        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"TransactionsReport_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var service = new MoneyRules.Application.Services.PdfReportService();
                    if (_currentUser == null)
                    {
                        MessageBox.Show("Користувач не знайдений. Експорт неможливий.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    service.CreateTransactionsReport(dlg.FileName, _currentUser.UserId);
                    MessageBox.Show("PDF збережено успішно.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Помилка при створенні PDF: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
