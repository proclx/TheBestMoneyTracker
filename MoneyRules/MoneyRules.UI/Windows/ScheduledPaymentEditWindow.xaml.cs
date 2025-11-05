using System.Windows;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using System.Threading.Tasks;
using System.Linq;

namespace MoneyRules.UI.Windows
{
 public partial class ScheduledPaymentEditWindow : Window
 {
 private readonly IScheduledPaymentService _service;
 private readonly int? _editingId;
 private ScheduledPayment? _entity;

 public ScheduledPaymentEditWindow(IScheduledPaymentService service, int? editingId = null)
 {
 _service = service;
 _editingId = editingId;
 InitializeComponent();
 Loaded += ScheduledPaymentEditWindow_Loaded;
 }

 private async void ScheduledPaymentEditWindow_Loaded(object sender, RoutedEventArgs e)
 {
 // load categories for combo
 var app = global::System.Windows.Application.Current;
 if (app != null && app.Properties["CurrentUser"] is User user)
 {
 var cats = await _service.GetScheduledPaymentsAsync(user.UserId); // temporary; we need categories from transaction service
 // Note: small workaround: We'll populate category combobox via DbContext (quick access)
 var db = (MoneyRules.Infrastructure.Persistence.AppDbContext) (app as App)!.ServiceProvider!.GetService(typeof(MoneyRules.Infrastructure.Persistence.AppDbContext));
 var categories = db.Categories.Where(c => c.UserId == user.UserId).ToList();
 CmbCategory.ItemsSource = categories;
 CmbCategory.DisplayMemberPath = "Name";
 CmbCategory.SelectedValuePath = "CategoryId";

 if (_editingId.HasValue)
 {
 // load existing scheduled payment
 _entity = db.ScheduledPayments.Find(_editingId.Value);
 if (_entity != null)
 {
 TxtDescription.Text = _entity.Description;
 TxtAmount.Text = _entity.Amount.ToString();
 TxtInterval.Text = _entity.Interval.ToString();
 DpStart.SelectedDate = _entity.StartDate;
 DpEnd.SelectedDate = _entity.EndDate;
 // frequency
 CmbFrequency.SelectedIndex = (int)_entity.Frequency;
 if (_entity.CategoryId.HasValue)
 {
 CmbCategory.SelectedValue = _entity.CategoryId.Value;
 }
 }
 }
 else
 {
 // new entity defaults
 DpStart.SelectedDate = System.DateTime.Now;
 CmbFrequency.SelectedIndex =3; // Monthly default
 TxtInterval.Text = "1";
 }
 }
 }

 private async void Save_Click(object sender, RoutedEventArgs e)
 {
 var app = global::System.Windows.Application.Current;
 if (app == null || app.Properties["CurrentUser"] is not User user)
 {
 MessageBox.Show("No user loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
 return;
 }

 if (_entity == null)
 {
 _entity = new ScheduledPayment();
 _entity.UserId = user.UserId;
 }

 _entity.Description = TxtDescription.Text;
 if (!decimal.TryParse(TxtAmount.Text, out var amt))
 {
 MessageBox.Show("Invalid amount", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
 return;
 }
 _entity.Amount = amt;
 _entity.Frequency = (MoneyRules.Domain.Enums.ScheduledPaymentFrequency)CmbFrequency.SelectedIndex;
 if (!int.TryParse(TxtInterval.Text, out var interval)) interval =1;
 _entity.Interval = interval;
 _entity.StartDate = DpStart.SelectedDate ?? System.DateTime.Now;
 _entity.EndDate = DpEnd.SelectedDate;
 if (CmbCategory.SelectedValue is int cid) _entity.CategoryId = cid;

 // save via service
 if (_editingId.HasValue)
 {
 await _service.UpdateAsync(_entity);
 }
 else
 {
 await _service.CreateAsync(_entity);
 }

 this.DialogResult = true;
 this.Close();
 }

 private void Cancel_Click(object sender, RoutedEventArgs e)
 {
 this.DialogResult = false;
 this.Close();
 }
 }
}
