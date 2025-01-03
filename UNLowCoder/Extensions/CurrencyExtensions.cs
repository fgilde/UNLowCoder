using Nextended.Core.Types;
using System;

namespace UNLowCoder.Core.Extensions;

public static class CurrencyExtensions
{
    public static Money CreateMoney(this Currency currency, decimal amount) 
        => new(amount, currency);

    public static Money ConvertAmount(this Currency currency, decimal amount, Currency targetCurrency, DateTime? currencyRateTargetDate = null) 
        => currency.CreateMoney(amount).ConvertCurrency(targetCurrency, currencyRateTargetDate ?? DateTime.UtcNow.AddDays(-1));
}