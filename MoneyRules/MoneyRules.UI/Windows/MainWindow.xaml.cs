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
using Microsoft.Extensions.DependencyInjection; 
using System;
using System.Collections.Generic;
using MoneyRules.UI.Utils;
using MoneyRules.UI;

namespace MoneyRules.UI.Windows
{
    public partial class MainWindow : Window
    {
        private readonly ITransactionService _transactionService;
        private readonly IAuthService _authService;
        private readonly IAdviceService _adviceService;
        private readonly IUserProfileService _profileService;
        private readonly IChartService _chartService;
        private readonly ICurrencyService _currencyService;
        private readonly IFileUploadService _fileUploadService;
        private User? _currentUser;
        

        public MainWindow(
            ITransactionService transactionService,
            IAuthService authService,
            IUserProfileService profileService,
            IAdviceService adviceService,
            IChartService chartService,
            ICurrencyService currencyService,
            IFileUploadService fileUploadService)
        {
            InitializeComponent();

            _transactionService = transactionService;
            _authService = authService;
            _adviceService = adviceService;
            _profileService = profileService;
            _chartService = chartService;
            _currencyService = currencyService;
            _fileUploadService = fileUploadService;

            _currentUser = System.Windows.Application.Current.Properties["CurrentUser"] as User;
            if (_currentUser == null)
            {
                MessageBox.Show("Користувач не знайдений. Увійдіть знову.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            LoadUserProfile();
            LoadAdvice();
            LoadExchangeRates();
            PopulateChartMonths();

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

        // --- ПОЧАТОК НОВОГО КОДУ ---
        /* * * * Це новий метод, який відкриває НОВЕ вікно для дублювання.
         */
        private void OpenDuplicatePaymentWindow_Click(object sender, RoutedEventArgs e)
        {
            DuplicatePaymentWindow duplicateWindow = new DuplicatePaymentWindow();
            duplicateWindow.Owner = this; 
            duplicateWindow.ShowDialog();
        }
        // --- КІНЕЦЬ НОВОГО КОДУ ---

        private void LoadAdvice()
        {
            try
            {
                if (_currentUser == null)
                    return;
                var tips = _adviceService.GetAdviceForUser(_currentUser.UserId);
                var adviceList = FindName("AdviceList") as ItemsControl;
                if (adviceList != null)
                {
                    adviceList.ItemsSource = tips.Select(t => new TextBlock
                    {
                        Text = t,
                        TextWrapping = TextWrapping.Wrap
                        , Foreground = (System.Windows.Media.Brush)FindResource("SecondaryText")
                    });
                }
            }
            catch (Exception ex)
            {
                var adviceList = FindName("AdviceList") as ItemsControl;
                if (adviceList != null)
                {
                    adviceList.ItemsSource = new[]
                    { new TextBlock { Text = "Не вдалося отримати поради: " + ex.Message } };
                }
            }
        }


        private void RefreshAdviceButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAdvice();
        }

        private void LoadUserProfile()
        {
            var txtName = FindName("TxtName") as TextBox;
            var txtEmail = FindName("TxtEmail") as TextBox;
            if (txtName != null) txtName.Text = _currentUser!.Name;
            if (txtEmail != null) txtEmail.Text = _currentUser!.Email;

            if (_currentUser!.ProfilePhoto != null && _currentUser.ProfilePhoto.Length > 0)
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
                
                var themeToggle = FindName("ThemeToggle") as CheckBox;
                if (themeToggle != null)
                {
                    themeToggle.IsChecked = _currentUser.Settings.Theme.Equals("Dark", StringComparison.OrdinalIgnoreCase);
                }

                var txtMonthlyBudget = FindName("TxtMonthlyBudget") as TextBox;
                if (txtMonthlyBudget != null)
                {
                    txtMonthlyBudget.Text = _currentUser.Settings.MonthlyBudget.ToString("N2");
                }
            }
            
            PopulateChartYears();
            DrawChartForSelectedYear();
        }

        private void BtnSetBudget_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Користувач не завантажений.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var txtMonthlyBudget = FindName("TxtMonthlyBudget") as TextBox;
            if (txtMonthlyBudget == null) return;

            if (decimal.TryParse(txtMonthlyBudget.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out decimal newBudget))
            {
                try
                {
                    _profileService.UpdateUserBudget(_currentUser.UserId, newBudget);
                    if (_currentUser.Settings == null)
                    {
                        _currentUser.Settings = new Settings { UserId = _currentUser.UserId };
                    }
                    _currentUser.Settings.MonthlyBudget = newBudget;
                    MessageBox.Show("Бюджет успішно збережено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    DrawChartForSelectedYear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка збереження бюджету: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, введіть коректне числове значення для бюджету (наприклад, 5000.50).", "Невірний формат", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PopulateChartYears()
        {
            try
            {
                var cmb = FindName("CmbChartYear") as ComboBox;
                cmb?.Items.Clear();
                var years = _chartService.GetTransactionYears(_currentUser!.UserId);

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
            catch { /* ignore silently */ }
        }

        private void PopulateChartMonths()
        {
            try
            {
                var cmb = FindName("CmbChartMonth") as ComboBox;
                cmb?.Items.Clear();

                if (cmb != null)
                {
                    for (int m = 1; m <= 12; m++)
                    {
                        var name = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m);
                        cmb.Items.Add(new ComboBoxItem { Content = name, Tag = m });
                    }

                    cmb.SelectedIndex = DateTime.Now.Month - 1;
                }
            }
            catch { /* ignore silently */ }
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

            if (chart == null) return;
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

            var chkMonthly = FindName("ChkMonthlyView") as CheckBox;
            if (chkMonthly != null && chkMonthly.IsChecked == true)
            {
                var cmbMonth = FindName("CmbChartMonth") as ComboBox;
                int month = DateTime.Now.Month;
                if (cmbMonth != null && cmbMonth.SelectedItem is ComboBoxItem ms && ms.Tag is int mt)
                    month = mt;

                var dailyStats = _chartService.GetDailyStatistics(_currentUser.UserId, year, month);
                var days = dailyStats
                    .Where(kvp => kvp.Value.Income != 0m || kvp.Value.Expense != 0m)
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => new { Day = kvp.Key, Income = kvp.Value.Income, Expense = kvp.Value.Expense })
                    .ToList();

                var monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

                if (days.Count == 0)
                {
                    if (txtStatus != null) txtStatus.Text = $"Немає транзакцій за {monthName} {year}";
                    return;
                }

                var maxVal = Math.Max((double)days.Max(d => d.Income), (double)days.Max(d => d.Expense));
                if (maxVal < 1) maxVal = 1;

                double canvasW = chart.ActualWidth > 0 ? chart.ActualWidth : chart.Width;
                double canvasH = chart.ActualHeight > 0 ? chart.ActualHeight : chart.Height;
                int count = days.Count;
                double barWidth = (canvasW - 40) / Math.Max(1, count);

                for (int i = 0; i < count; i++)
                {
                    var d = days[i];
                    double x = 20 + i * barWidth;

                    double hInc = (double)d.Income / maxVal * (canvasH - 60);
                    var rectInc = new System.Windows.Shapes.Rectangle { Width = barWidth * 0.4, Height = Math.Max(1, hInc), Fill = System.Windows.Media.Brushes.Green, Stroke = System.Windows.Media.Brushes.Black };
                    Canvas.SetLeft(rectInc, x + barWidth * 0.05);
                    Canvas.SetTop(rectInc, canvasH - 30 - rectInc.Height);
                    chart.Children.Add(rectInc);

                    var incText = ((decimal)d.Income).ToString("N0");
                    var incLabel = new TextBlock { Text = incText, FontSize = 11, FontWeight = System.Windows.FontWeights.SemiBold, Foreground = System.Windows.Media.Brushes.White };
                    var incBorder = new System.Windows.Controls.Border { Background = System.Windows.Media.Brushes.DarkGreen, CornerRadius = new CornerRadius(4), Child = incLabel, Padding = new Thickness(6, 2, 6, 2), Opacity = d.Income == 0 ? 0.7 : 1 };
                    incBorder.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                    double incW = incBorder.DesiredSize.Width;
                    double incLeft = x + barWidth * 0.05 + (barWidth * 0.4 - incW) / 2;
                    Canvas.SetLeft(incBorder, incLeft);
                    Canvas.SetTop(incBorder, Math.Max(4, canvasH - 36 - rectInc.Height - incBorder.DesiredSize.Height));
                    chart.Children.Add(incBorder);

                    double hExp = (double)d.Expense / maxVal * (canvasH - 60);
                    var rectExp = new System.Windows.Shapes.Rectangle { Width = barWidth * 0.4, Height = Math.Max(1, hExp), Fill = System.Windows.Media.Brushes.Red, Stroke = System.Windows.Media.Brushes.Black };
                    Canvas.SetLeft(rectExp, x + barWidth * 0.55);
                    Canvas.SetTop(rectExp, canvasH - 30 - rectExp.Height);
                    chart.Children.Add(rectExp);

                    var expText = ((decimal)d.Expense).ToString("N0");
                    var expLabel = new TextBlock { Text = expText, FontSize = 11, FontWeight = System.Windows.FontWeights.SemiBold, Foreground = System.Windows.Media.Brushes.White };
                    var expBorder = new System.Windows.Controls.Border { Background = System.Windows.Media.Brushes.DarkRed, CornerRadius = new CornerRadius(4), Child = expLabel, Padding = new Thickness(6, 2, 6, 2), Opacity = d.Expense == 0 ? 0.7 : 1 };
                    expBorder.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                    double expW = expBorder.DesiredSize.Width;
                    double expLeft = x + barWidth * 0.55 + (barWidth * 0.4 - expW) / 2;
                    Canvas.SetLeft(expBorder, expLeft);
                    Canvas.SetTop(expBorder, Math.Max(4, canvasH - 36 - rectExp.Height - expBorder.DesiredSize.Height));
                    chart.Children.Add(expBorder);

                    var dayLbl = new TextBlock { Text = d.Day.ToString(), FontSize = 10 };
                    Canvas.SetLeft(dayLbl, x + barWidth * 0.1);
                    Canvas.SetTop(dayLbl, canvasH - 12);
                    chart.Children.Add(dayLbl);
                }

                if (txtStatus != null) txtStatus.Text = $"Дохід/Витрати за {monthName} {year}";
                return;
            }

            var monthlyStats = _chartService.GetMonthlyStatistics(_currentUser.UserId, year);
            var months = monthlyStats.Select(kvp => new { Month = kvp.Key, Income = kvp.Value.Income, Expense = kvp.Value.Expense }).ToList();

            var maxVal2 = Math.Max((double)months.Max(m => m.Income), (double)months.Max(m => m.Expense));
            if (maxVal2 < 1) maxVal2 = 1;

            double canvasW2 = chart.ActualWidth > 0 ? chart.ActualWidth : chart.Width;
            double canvasH2 = chart.ActualHeight > 0 ? chart.ActualHeight : chart.Height;
            double barWidth2 = (canvasW2 - 40) / 12.0;
            
            decimal monthlyBudget = _currentUser.Settings?.MonthlyBudget ?? 0m;
            if (monthlyBudget > 0 && maxVal2 > 0)
            {
                double budgetY = canvasH2 - 30 - ((double)monthlyBudget / maxVal2 * (canvasH2 - 60));
                if (budgetY > 4 && budgetY < (canvasH2 - 30))
                {
                    var budgetLine = new System.Windows.Shapes.Line { X1 = 20, Y1 = budgetY, X2 = canvasW2 - 20, Y2 = budgetY, Stroke = (System.Windows.Media.Brush)FindResource("PrimaryText"), StrokeThickness = 2, StrokeDashArray = new System.Windows.Media.DoubleCollection(new double[] { 4, 2 }) };
                    chart.Children.Add(budgetLine);
                    var budgetLabel = new TextBlock { Text = $"Бюджет: {monthlyBudget:N0}", Foreground = (System.Windows.Media.Brush)FindResource("SecondaryText"), FontSize = 10, FontStyle = FontStyles.Italic, Background = (System.Windows.Media.Brush)FindResource("AppBackground") };
                    Canvas.SetLeft(budgetLabel, 25);
                    Canvas.SetTop(budgetLabel, budgetY - 15);
                    chart.Children.Add(budgetLabel);
                }
            }

            for (int i = 0; i < 12; i++)
            {
                var m = months[i];
                double x = 20 + i * barWidth2;

                double hInc = (double)m.Income / maxVal2 * (canvasH2 - 60);
                var rectInc = new System.Windows.Shapes.Rectangle { Width = barWidth2 * 0.4, Height = Math.Max(1, hInc), Fill = System.Windows.Media.Brushes.Green, Stroke = System.Windows.Media.Brushes.Black };
                Canvas.SetLeft(rectInc, x + barWidth2 * 0.05);
                Canvas.SetTop(rectInc, canvasH2 - 30 - rectInc.Height);
                chart.Children.Add(rectInc);

                var incText2 = ((decimal)m.Income).ToString("N0");
                var incLabel2 = new TextBlock { Text = incText2, FontSize = 11, FontWeight = System.Windows.FontWeights.SemiBold, Foreground = System.Windows.Media.Brushes.White };
                var incBorder2 = new System.Windows.Controls.Border { Background = System.Windows.Media.Brushes.DarkGreen, CornerRadius = new CornerRadius(4), Child = incLabel2, Padding = new Thickness(6, 2, 6, 2), Opacity = m.Income == 0 ? 0.7 : 1 };
                incBorder2.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                double incW2 = incBorder2.DesiredSize.Width;
                double incLeft2 = x + barWidth2 * 0.05 + (barWidth2 * 0.4 - incW2) / 2;
                Canvas.SetLeft(incBorder2, incLeft2);
                Canvas.SetTop(incBorder2, Math.Max(4, canvasH2 - 36 - rectInc.Height - incBorder2.DesiredSize.Height));
                chart.Children.Add(incBorder2);

                double hExp = (double)m.Expense / maxVal2 * (canvasH2 - 60);
                var rectExp = new System.Windows.Shapes.Rectangle { Width = barWidth2 * 0.4, Height = Math.Max(1, hExp), Fill = System.Windows.Media.Brushes.Red, Stroke = System.Windows.Media.Brushes.Black };
                Canvas.SetLeft(rectExp, x + barWidth2 * 0.55);
                Canvas.SetTop(rectExp, canvasH2 - 30 - rectExp.Height);
                chart.Children.Add(rectExp);

                var expText2 = ((decimal)m.Expense).ToString("N0");
                var expLabel2 = new TextBlock { Text = expText2, FontSize = 11, FontWeight = System.Windows.FontWeights.SemiBold, Foreground = System.Windows.Media.Brushes.White };
                var expBorder2 = new System.Windows.Controls.Border { Background = System.Windows.Media.Brushes.DarkRed, CornerRadius = new CornerRadius(4), Child = expLabel2, Padding = new Thickness(6, 2, 6, 2), Opacity = m.Expense == 0 ? 0.7 : 1 };
                expBorder2.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                double expW2 = expBorder2.DesiredSize.Width;
                double expLeft2 = x + barWidth2 * 0.55 + (barWidth2 * 0.4 - expW2) / 2;
                Canvas.SetLeft(expBorder2, expLeft2);
                Canvas.SetTop(expBorder2, Math.Max(4, canvasH2 - 36 - rectExp.Height - expBorder2.DesiredSize.Height));
                chart.Children.Add(expBorder2);

                var monthLbl = new TextBlock { Text = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i + 1), FontSize = 10 };
                Canvas.SetLeft(monthLbl, x + barWidth2 * 0.1);
                Canvas.SetTop(monthLbl, canvasH2 - 12);
                chart.Children.Add(monthLbl);
            }

            if (txtStatus != null) txtStatus.Text = $"Дохід/Витрати за {year}";
        }

        private void ChkMonthlyView_CheckedChanged(object sender, RoutedEventArgs e)
        {
            DrawChartForSelectedYear();
        }

        private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Image Files|*.jpg;*.png;*.jpeg" };
            if (dlg.ShowDialog() == true)
            {
                byte[] imageData = File.ReadAllBytes(dlg.FileName);
                _profileService.ChangeProfilePhoto(_currentUser!, imageData);
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
            if (_currentUser == null) { MessageBox.Show("User not loaded."); return; }

            var txtName2 = FindName("TxtName") as TextBox;
            var txtEmail2 = FindName("TxtEmail") as TextBox;
            if (txtName2 != null) _currentUser.Name = txtName2.Text;
            if (txtEmail2 != null) _currentUser.Email = txtEmail2.Text;

            var cmbCurrency2 = FindName("CmbCurrency") as ComboBox;
            var selectedCurrency = (cmbCurrency2?.SelectedItem as ComboBoxItem)?.Content.ToString() ?? _currentUser.Settings?.Currency ?? "UAH";

            if (_currentUser.Settings == null)
                _currentUser.Settings = new Settings { UserId = _currentUser.UserId };

            _currentUser.Settings.Currency = selectedCurrency;
            var chkNotifications2 = FindName("ChkNotifications") as CheckBox;
            _currentUser.Settings.NotificationEnabled = chkNotifications2?.IsChecked ?? false;
            
            var themeToggle = FindName("ThemeToggle") as CheckBox;
            if (themeToggle != null)
            {
                _currentUser.Settings.Theme = (themeToggle.IsChecked == true) ? "Dark" : "Light";
            }
            
            _profileService.UpdateUser(_currentUser);
            MessageBox.Show("Profile updated successfully.");
        }
        

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Ви впевнені, що хочете вийти з акаунту?", "Підтвердження виходу", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Properties["CurrentUser"] = null;
                var serviceProvider = (System.Windows.Application.Current as App)!.ServiceProvider;
                if (serviceProvider == null) { MessageBox.Show("Критична помилка: ServiceProvider не знайдено."); return; }
                
                var welcomeWindow = serviceProvider.GetRequiredService<WelcomeWindow>();
                welcomeWindow.Show();
                this.Close();
            }
        }

        private async void LoadExchangeRates()
        {
            try
            {
                var rates = await _currencyService.GetCurrentExchangeRatesAsync();
                var txtRates = FindName("TxtExchangeRates") as TextBlock;
                
                if (txtRates != null)
                {
                    var ratesList = new List<string>();
                    foreach (var rate in rates)
                    {
                        if (rate.Currency == "USD" || rate.Currency == "EUR")
                        {
                            if (decimal.TryParse(rate.BuyRate.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal buyRate) &&
                                decimal.TryParse(rate.SaleRate.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal sellRate))
                            {
                                ratesList.Add($"💱 {rate.Currency}/{rate.BaseCurrency}\n" +
                                                $"▪ Купівля:  {buyRate:N2}\n" +
                                                $"▪ Продаж:   {sellRate:N2}");
                            }
                        }
                    }
                    txtRates.Text = string.Join("\n\n", ratesList);
                }
            }
            catch (Exception ex)
            {
                var txtRates = FindName("TxtExchangeRates") as TextBlock;
                if (txtRates != null) { txtRates.Text = $"Помилка завантаження курсів валют: {ex.Message}"; }
            }
        }

        private void BtnRefreshRates_Click(object sender, RoutedEventArgs e)
        {
            LoadExchangeRates();
        }

        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", FileName = $"TransactionsReport_{DateTime.Now:yyyyMMdd}.pdf" };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var service = new MoneyRules.Application.Services.PdfReportService();
                    if (_currentUser == null) { MessageBox.Show("Користувач не знайдений. Експорт неможливий.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                    service.CreateTransactionsReport(dlg.FileName, _currentUser.UserId);
                    MessageBox.Show("PDF збережено успішно.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Помилка при створенні PDF: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) { MessageBox.Show("Користувач не завантажений.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            var serviceProvider = (System.Windows.Application.Current as App)!.ServiceProvider;
            if (serviceProvider == null) { /* ... обробка помилки ... */ return; }
            var changePasswordWindow = new ChangePasswordWindow(_authService, _currentUser);
            changePasswordWindow.Owner = this;
            changePasswordWindow.ShowDialog();
        }
        
        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.SwitchTheme(Theme.Dark);
        }

        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ThemeManager.SwitchTheme(Theme.Light);
        }
        
        private void BtnScheduledPayments_Click(object sender, RoutedEventArgs e)
        {
            var app = System.Windows.Application.Current as App;
            if (app == null || app.ServiceProvider == null) { MessageBox.Show("Не вдалося отримати доступ до сервісів додатку.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            var window = app.ServiceProvider.GetService(typeof(ScheduledPaymentsWindow)) as ScheduledPaymentsWindow;
            if (window == null) { MessageBox.Show("Служба вікна не зареєстрована.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            window.Owner = this;
            window.ShowDialog();
        }

        private void BtnDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            var deleteWindow = new MoneyRules.UI.Windows.DeleteAccountWindow();
            deleteWindow.ShowDialog();
        }

    }
}