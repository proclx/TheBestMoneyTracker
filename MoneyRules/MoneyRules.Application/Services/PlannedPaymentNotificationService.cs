using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MoneyRules.Application.Services
{
    public class PlannedPaymentNotificationService : IPlannedPaymentNotificationService
    {
        private readonly AppDbContext _context;

        public PlannedPaymentNotificationService(AppDbContext context)
        {
            _context = context;
        }

        public List<ScheduledPayment> GetUpcomingPayments(int userId, int daysAhead = 7)
        {
            var today = DateTime.Today;
            var futureDate = today.AddDays(daysAhead);

            // Фільтруємо активні платежі для поточного користувача.
            var allActivePayments = _context.ScheduledPayments
                .Where(p => p.UserId == userId && p.IsActive) 
                .Include(p => p.Category)
                .ToList(); 

            var upcomingPayments = new List<ScheduledPayment>();

            foreach (var payment in allActivePayments)
            {
                // Обчислюємо наступну дату платежу.
                DateTime nextDueDate = CalculateNextDueDate(payment, today);
                
                // Перевіряємо, чи ця дата знаходиться в межах діапазону сповіщення
                if (nextDueDate >= today && nextDueDate <= futureDate)
                {
                    upcomingPayments.Add(payment);
                }
            }

            return upcomingPayments.OrderBy(p => p.StartDate).ToList();
        }
        
        /// <summary>
        /// Обчислює наступну дату, коли платіж має бути виконаний, починаючи з сьогодні.
        /// Ця логіка виправлена для коректної роботи з платежами "на сьогодні" та повтореннями.
        /// </summary>
        private DateTime CalculateNextDueDate(ScheduledPayment payment, DateTime searchStart)
        {
            DateTime current = payment.StartDate.Date; // Використовуємо лише дату
            
            bool isRepeating = payment.Frequency == ScheduledPaymentFrequency.Daily ||
                              payment.Frequency == ScheduledPaymentFrequency.Weekly ||
                              payment.Frequency == ScheduledPaymentFrequency.Monthly ||
                              payment.Frequency == ScheduledPaymentFrequency.Yearly;

            // Якщо це одноразовий платіж, і його StartDate вже минула, його ігноруємо.
            if (!isRepeating && current < searchStart)
            {
                return DateTime.MinValue; 
            }
            
            // Пересуваємо current, поки він не досягне або не перевищить searchStart.
            while (current < searchStart)
            {
                // Перевірка на EndDate
                if (payment.EndDate.HasValue && current > payment.EndDate.Value.Date)
                {
                    return DateTime.MinValue;
                }

                int interval = Math.Max(1, payment.Interval);

                switch (payment.Frequency)
                {
                    case ScheduledPaymentFrequency.Daily:
                        current = current.AddDays(interval);
                        break;
                    case ScheduledPaymentFrequency.Weekly:
                        current = current.AddDays(7 * interval);
                        break;
                    case ScheduledPaymentFrequency.Monthly:
                        try
                        {
                            current = current.AddMonths(interval);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            current = current.AddDays(1).AddMonths(interval).AddDays(-1); 
                        }
                        break;
                    case ScheduledPaymentFrequency.Yearly:
                        current = current.AddYears(interval);
                        break;
                    default:
                        // Якщо платіж одноразовий і не пройшов перевірку current < searchStart
                        return payment.StartDate.Date;
                }
            }
            // Повертаємо обчислену дату, яка знаходиться сьогодні або в майбутньому.
            return current;
        }
    }
}