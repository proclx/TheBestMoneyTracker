using System;
using System.IO;
using System.Linq;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using MoneyRules.Infrastructure.Persistence;

namespace MoneyRules.Application.Services
{
    public class PdfReportService
    {
        private readonly Func<AppDbContext> _dbFactory;

        // Allow injection of a factory for testing; default creates AppDbContext
        public PdfReportService(Func<AppDbContext>? dbFactory = null)
        {
            _dbFactory = dbFactory ?? (() => new AppDbContext());
        }

        public void CreateTransactionsReport(string filePath, int? userId = null)
        {
            using var db = _dbFactory();
            var query = db.Transactions.AsQueryable();
            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            var transactions = query.OrderBy(t => t.Date).ToList();

            var groupedByMonth = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Income = g.Where(t => t.Type.ToString().ToLower().Contains("income")).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type.ToString().ToLower().Contains("expense")).Sum(t => t.Amount),
                    Count = g.Count(),
                    AvgPerDay = g.Sum(t => t.Amount) / DateTime.DaysInMonth(g.Key.Year, g.Key.Month)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            var totalIncome = transactions.Where(t => t.Type.ToString().ToLower().Contains("income")).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Type.ToString().ToLower().Contains("expense")).Sum(t => t.Amount);
            var netTotal = totalIncome - totalExpense;

            // Ensure directory
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);

            var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
            var fontHeader = new XFont("Arial", 10, XFontStyle.Bold);
            var font = new XFont("Arial", 9, XFontStyle.Regular);

            double y = 40;
            gfx.DrawString("Звіт по транзакціях", fontTitle, XBrushes.Black, 40, y);
            y += 30;
            gfx.DrawString($"Сформовано: {DateTime.Now:yyyy-MM-dd HH:mm}", font, XBrushes.Black, 40, y);
            y += 18;
            gfx.DrawString($"Кількість транзакцій: {transactions.Count}", font, XBrushes.Black, 40, y);
            y += 18;
            gfx.DrawString($"Дохід: {totalIncome:C}", fontHeader, XBrushes.Black, 40, y);
            gfx.DrawString($"Витрати: {totalExpense:C}", fontHeader, XBrushes.Black, 220, y);
            gfx.DrawString($"Чистий: {netTotal:C}", fontHeader, XBrushes.Black, 420, y);
            y += 25;

            // Month summary table header
            gfx.DrawString("Місяць", fontHeader, XBrushes.Black, 40, y);
            gfx.DrawString("Дохід", fontHeader, XBrushes.Black, 180, y);
            gfx.DrawString("Витрати", fontHeader, XBrushes.Black, 260, y);
            gfx.DrawString("Кільк.", fontHeader, XBrushes.Black, 340, y);
            gfx.DrawString("Середньо/день", fontHeader, XBrushes.Black, 400, y);
            y += 18;

            foreach (var m in groupedByMonth)
            {
                gfx.DrawString(new DateTime(m.Year, m.Month, 1).ToString("yyyy MMM"), font, XBrushes.Black, 40, y);
                gfx.DrawString(m.Income.ToString("C"), font, XBrushes.Black, 180, y);
                gfx.DrawString(m.Expense.ToString("C"), font, XBrushes.Black, 260, y);
                gfx.DrawString(m.Count.ToString(), font, XBrushes.Black, 340, y);
                gfx.DrawString(m.AvgPerDay.ToString("C"), font, XBrushes.Black, 400, y);
                y += 16;
                if (y > page.Height - 100)
                {
                    page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            y += 20;
                gfx.DrawString("Детальні транзакції:", fontHeader, XBrushes.Black, 40, y);
            y += 18;

            foreach (var t in transactions)
            {
                string typeLabel = (t.Type.ToString().ToLower().Contains("expense")) ? "Витрата" : "Дохід";
                var line = $"{t.Date:yyyy-MM-dd} | {typeLabel} | {t.Category?.Name ?? t.Description} | {t.Amount:C} | {t.Description}";
                gfx.DrawString(line, font, XBrushes.Black, 40, y);
                y += 14;
                if (y > page.Height - 60)
                {
                    page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            using var fs = File.Open(filePath, FileMode.Create);
            document.Save(fs);
        }
    }
}
