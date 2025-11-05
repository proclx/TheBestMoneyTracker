using MoneyRules.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyRules.Application.Interfaces
{
    // Цей клас будемо повертати, щоб UI знав про результат
    public class ImportResult
    {
        public bool Success { get; set; }
        public int ImportedCount { get; set; }
        
        // ВИПРАВЛЕННЯ: Ініціалізуємо, щоб уникнути попередження CS8618
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public interface IFileUploadService
    {
        // Метод для стандартного CSV
        Task<ImportResult> ImportDefaultCsvAsync(string[] lines, int userId);
        
        // Метод для Monobank CSV
        Task<ImportResult> ImportMonobankCsvAsync(string[] lines, int userId);
    }
}