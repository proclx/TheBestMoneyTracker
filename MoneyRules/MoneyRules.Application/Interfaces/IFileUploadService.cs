using MoneyRules.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyRules.Application.Interfaces
{
    public class ImportResult
    {
        public bool Success { get; set; }
        public int ImportedCount { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public interface IFileUploadService
    {
        Task<ImportResult> ImportDefaultCsvAsync(string[] lines, int userId);
        Task<ImportResult> ImportMonobankCsvAsync(string[] lines, int userId);
        
        Task<ImportResult> ImportPrivatBankXlsxAsync(string filePath, int userId);
    }
}