using MoneyRules.Domain.Entities;
using System.Collections.Generic;

namespace MoneyRules.Application.Interfaces
{
    // Інтерфейс для отримання запланованих платежів, термін яких наближається.
    public interface IPlannedPaymentNotificationService
    {
        // Отримує заплановані платежі для певного користувача, які мають настати протягом днів.
        List<ScheduledPayment> GetUpcomingPayments(int userId, int daysAhead = 7);
    }
}