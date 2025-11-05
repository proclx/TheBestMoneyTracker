using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;

namespace MoneyRules.Tests.Tests
{
 public class ScheduledPaymentServiceTests
 {
 private AppDbContext CreateContext() => TestDbContextFactory.CreateInMemoryDb();

 [Fact]
 public async Task CreateAsync_SavesEntityAndNormalizesDatesToUtc()
 {
 var context = CreateContext();
 var service = new ScheduledPaymentService(context);

 var user = new User { Name = "U", Email = "u@example.com", PasswordHash = "h", Role = UserRole.User, ProfilePhoto = Array.Empty<byte>() };
 context.Users.Add(user);
 await context.SaveChangesAsync();

 var localStart = DateTime.SpecifyKind(DateTime.Now.AddDays(1), DateTimeKind.Local);
 var unspecifiedEnd = DateTime.SpecifyKind(DateTime.Now.AddDays(10), DateTimeKind.Unspecified);

 var sp = new ScheduledPayment
 {
 UserId = user.UserId,
 Amount =123.45m,
 Description = "Test",
 StartDate = localStart,
 EndDate = unspecifiedEnd,
 Frequency = ScheduledPaymentFrequency.Monthly,
 Interval =1,
 IsActive = true
 };

 var created = await service.CreateAsync(sp);

 Assert.True(created.ScheduledPaymentId >0);

 var fromDb = context.ScheduledPayments.Find(created.ScheduledPaymentId);
 Assert.NotNull(fromDb);

 Assert.Equal(DateTimeKind.Utc, created.StartDate.Kind);
 Assert.Equal(DateTimeKind.Utc, created.EndDate?.Kind);

 // Values should match (in UTC)
 Assert.Equal(created.StartDate, fromDb.StartDate);
 Assert.Equal(created.EndDate, fromDb.EndDate);
 }

 [Fact]
 public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
 {
 var context = CreateContext();
 var service = new ScheduledPaymentService(context);

 var result = await service.DeleteAsync(9999);

 Assert.False(result);
 }

 [Fact]
 public async Task DeleteAsync_RemovesExistingScheduledPayment()
 {
 var context = CreateContext();
 var service = new ScheduledPaymentService(context);

 var user = new User { Name = "U2", Email = "u2@example.com", PasswordHash = "h", Role = UserRole.User, ProfilePhoto = Array.Empty<byte>() };
 context.Users.Add(user);
 await context.SaveChangesAsync();

 var sp = new ScheduledPayment
 {
 UserId = user.UserId,
 Amount =10m,
 Description = "Del",
 StartDate = DateTime.UtcNow,
 Frequency = ScheduledPaymentFrequency.OneTime,
 Interval =1,
 IsActive = true
 };
 context.ScheduledPayments.Add(sp);
 await context.SaveChangesAsync();

 var id = sp.ScheduledPaymentId;
 var deleted = await service.DeleteAsync(id);

 Assert.True(deleted);
 var fromDb = await context.ScheduledPayments.FindAsync(id);
 Assert.Null(fromDb);
 }

 [Fact]
 public async Task GetScheduledPaymentsAsync_ReturnsOnlyActiveForUser()
 {
 var context = CreateContext();
 var service = new ScheduledPaymentService(context);

 var userA = new User { Name = "A", Email = "a@example.com", PasswordHash = "h", Role = UserRole.User, ProfilePhoto = Array.Empty<byte>() };
 var userB = new User { Name = "B", Email = "b@example.com", PasswordHash = "h", Role = UserRole.User, ProfilePhoto = Array.Empty<byte>() };
 context.Users.AddRange(userA, userB);
 await context.SaveChangesAsync();

 var sp1 = new ScheduledPayment { UserId = userA.UserId, Amount =1, Description = "a1", StartDate = DateTime.UtcNow, Frequency = ScheduledPaymentFrequency.Daily, Interval =1, IsActive = true };
 var sp2 = new ScheduledPayment { UserId = userA.UserId, Amount =2, Description = "a2", StartDate = DateTime.UtcNow, Frequency = ScheduledPaymentFrequency.Daily, Interval =1, IsActive = false };
 var sp3 = new ScheduledPayment { UserId = userB.UserId, Amount =3, Description = "b1", StartDate = DateTime.UtcNow, Frequency = ScheduledPaymentFrequency.Daily, Interval =1, IsActive = true };

 context.ScheduledPayments.AddRange(sp1, sp2, sp3);
 await context.SaveChangesAsync();

 var results = (await service.GetScheduledPaymentsAsync(userA.UserId)).ToList();

 Assert.Single(results);
 Assert.Equal(sp1.Description, results[0].Description);
 }

 [Fact]
 public async Task UpdateAsync_UpdatesEntityAndNormalizesDates()
 {
 var context = CreateContext();
 var service = new ScheduledPaymentService(context);

 var user = new User { Name = "U3", Email = "u3@example.com", PasswordHash = "h", Role = UserRole.User, ProfilePhoto = Array.Empty<byte>() };
 context.Users.Add(user);
 await context.SaveChangesAsync();

 var sp = new ScheduledPayment
 {
 UserId = user.UserId,
 Amount =20m,
 Description = "Up",
 StartDate = DateTime.UtcNow,
 Frequency = ScheduledPaymentFrequency.Weekly,
 Interval =1,
 IsActive = true
 };
 context.ScheduledPayments.Add(sp);
 await context.SaveChangesAsync();

 // Modify with local kind and new amount
 sp.Amount =55m;
 sp.StartDate = DateTime.SpecifyKind(DateTime.Now.AddDays(2), DateTimeKind.Local);

 var updated = await service.UpdateAsync(sp);

 Assert.Equal(55m, updated.Amount);
 Assert.Equal(DateTimeKind.Utc, updated.StartDate.Kind);

 var fromDb = await context.ScheduledPayments.FindAsync(sp.ScheduledPaymentId);
 Assert.Equal(55m, fromDb.Amount);
 Assert.Equal(updated.StartDate, fromDb.StartDate);
 }
 }
}
