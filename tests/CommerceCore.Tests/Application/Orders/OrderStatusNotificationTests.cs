using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Application.Features.Orders;
using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Domain.Entities.Orders;
using CommerceCore.Domain.Enums;
using CommerceCore.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommerceCore.Tests.Application.Orders;

public class OrderStatusNotificationTests
{
    [Fact]
    public async Task Should_Send_Email_And_Sms_When_Order_Status_Transitions_To_Confirmed()
    {
        await using var db = CreateDbContext();

        var customer = new Customer
        {
            StoreId = Guid.NewGuid(),
            Email = "customer@example.com",
            Phone = "+15551234567",
            FirstName = "Jane",
            LastName = "Doe",
            IsGuest = false,
            IsActive = true
        };

        var order = new Order
        {
            StoreId = customer.StoreId,
            CustomerId = customer.Id,
            Customer = customer,
            OrderNumber = "TS-1001",
            CurrencyCode = "INR",
            Status = OrderStatus.Pending,
            PaymentStatus = OrderPaymentStatus.Paid,
            SubTotal = 100m,
            DiscountAmount = 0m,
            ShippingAmount = 0m,
            TaxAmount = 18m,
            TotalAmount = 118m,
            ShippingFullName = "Jane Doe",
            ShippingAddressLine1 = "1 Main Road",
            ShippingCity = "Bengaluru",
            ShippingState = "Karnataka",
            ShippingPostalCode = "560001",
            ShippingCountry = "IN",
            OrderItems =
            {
                new OrderItem
                {
                    ProductNameSnapshot = "Midnight Oud",
                    SkuSnapshot = "MOU-001",
                    UnitPrice = 100m,
                    Quantity = 1,
                    TaxAmount = 18m,
                    DiscountAmount = 0m
                }
            }
        };

        db.Customers.Add(customer);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var emailSender = new StubEmailSender();
        var smsSender = new StubSmsSender();
        var handler = new UpdateOrderStatusCommandHandler(db, emailSender, smsSender);

        var result = await handler.Handle(new UpdateOrderStatusCommand(order.Id, "Confirmed"), CancellationToken.None);

        result.Status.Should().Be("Confirmed");
        emailSender.Messages.Should().ContainSingle();
        emailSender.Messages[0].ToAddress.Should().Be("customer@example.com");
        emailSender.Messages[0].Body.Should().Contain("Midnight Oud");
        smsSender.Messages.Should().ContainSingle();
        smsSender.Messages[0].Should().Contain("Confirmed");
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private sealed class StubEmailSender : IEmailApiSender
    {
        public List<(string ToAddress, string Subject, string Body)> Messages { get; } = new();

        public Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            Messages.Add((toAddress, subject, htmlBody));
            return Task.CompletedTask;
        }
    }

    private sealed class StubSmsSender : ISmsSender
    {
        public List<string> Messages { get; } = new();

        public Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
