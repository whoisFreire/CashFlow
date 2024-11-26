using CashFlow.Domain.Enums;

namespace CashFlow.Domain.Extensions;

public static class PaymentTypeExtensions
{
    public static string PaymentTypeToString(this PaymentType paymentType)
    {

        return paymentType switch
        {
            PaymentType.Cash => "Dinheiro",
            PaymentType.DebitCard => "Débito",
            PaymentType.CreditCard => "Crédito",
            PaymentType.EletronicTransfer => "Transferência",
            _ => string.Empty,
        };

    }
}
