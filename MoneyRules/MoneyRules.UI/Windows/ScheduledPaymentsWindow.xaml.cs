using System.Windows;
using MoneyRules.Application.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using MoneyRules.Domain.Entities;
using System.Linq;

namespace MoneyRules.UI.Windows
{
    public partial class ScheduledPaymentsWindow : Window
    {
        private readonly IScheduledPaymentService _scheduledPaymentService;
        public ScheduledPaymentsWindow(IScheduledPaymentService scheduledPaymentService)
        {
            _scheduledPaymentService = scheduledPaymentService;
            InitializeComponent();
            Loaded += ScheduledPaymentsWindow_Loaded;
        }

        private async void ScheduledPaymentsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshListAsync();
        }

        private async Task RefreshListAsync()
        {
            var app = global::System.Windows.Application.Current;
            if (app != null && app.Properties["CurrentUser"] is User user)
            {
                var list = await _scheduledPaymentService.GetScheduledPaymentsAsync(user.UserId);
                PaymentsListView.ItemsSource = list.OrderBy(sp => sp.StartDate).ToList();
            }
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new ScheduledPaymentEditWindow(_scheduledPaymentService);
            wnd.Owner = this;
            if (wnd.ShowDialog() == true)
            {
                await RefreshListAsync();
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsListView.SelectedItem is ScheduledPayment sp)
            {
                var wnd = new ScheduledPaymentEditWindow(_scheduledPaymentService, sp.ScheduledPaymentId);
                wnd.Owner = this;
                if (wnd.ShowDialog() == true)
                {
                    await RefreshListAsync();
                }
            }
            else
            {
                MessageBox.Show("Select an item first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsListView.SelectedItem is ScheduledPayment sp)
            {
                var res = MessageBox.Show("Delete selected scheduled payment?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    var ok = await _scheduledPaymentService.DeleteAsync(sp.ScheduledPaymentId);
                    if (ok) await RefreshListAsync();
                }
            }
            else
            {
                MessageBox.Show("Select an item first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
