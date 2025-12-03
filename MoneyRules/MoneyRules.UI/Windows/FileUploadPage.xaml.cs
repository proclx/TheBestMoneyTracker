using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using MoneyRules.Domain.Entities;
using MoneyRules.Application.Interfaces; 
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MoneyRules.UI.Windows 
{
    public partial class FileUploadPage : UserControl
    {
        public FileUploadPage()
        {
            InitializeComponent();
            // Resolve ViewModel from application's service provider: pass IServiceProvider so VM creates short-lived scopes
            try
            {
                if (System.Windows.Application.Current is App app && app.ServiceProvider != null)
                {
                    DataContext = new MoneyRules.UI.ViewModel.FileUploadViewModel(app.ServiceProvider);
                }
            }
            catch { }
        }
    }
}