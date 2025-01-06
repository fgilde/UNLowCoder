using System;
using System.Collections.Generic;
using System.Linq;
using Nager.Country;

namespace UNLowCoder.Extensions;

public class CountryEqualityComparer : IEqualityComparer<ICountryInfo>
{
    public static CountryEqualityComparer Instance { get; } = new();
    public bool Equals(ICountryInfo x, ICountryInfo y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.CommonName == y.CommonName && x.OfficialName == y.OfficialName && x.NativeName == y.NativeName &&
               x.Alpha2Code == y.Alpha2Code && x.Alpha3Code == y.Alpha3Code && x.NumericCode == y.NumericCode &&
               x.TLD.SequenceEqual(y.TLD) && x.Region == y.Region && x.SubRegion == y.SubRegion &&
               x.BorderCountries.SequenceEqual(y.BorderCountries) && 
               x.Currencies.SequenceEqual(y.Currencies, NagerCurrencyComparer.Instance) &&
               x.CallingCodes.SequenceEqual(y.CallingCodes);
    }

    public int GetHashCode(ICountryInfo obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.CommonName);
        hashCode.Add(obj.OfficialName);
        hashCode.Add(obj.NativeName);
        hashCode.Add((int)obj.Alpha2Code);
        hashCode.Add((int)obj.Alpha3Code);
        hashCode.Add(obj.NumericCode);
        hashCode.Add(obj.TLD);
        hashCode.Add((int)obj.Region);
        hashCode.Add((int)obj.SubRegion);
        hashCode.Add(obj.BorderCountries);
        hashCode.Add(obj.Currencies);
        hashCode.Add(obj.CallingCodes);
        return hashCode.ToHashCode();
    }
}

public class NagerCurrencyComparer : IEqualityComparer<ICurrency>
{
    public static NagerCurrencyComparer Instance { get; } = new();

    public bool Equals(ICurrency x, ICurrency y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Symbol == y.Symbol && x.Singular == y.Singular && x.Plural == y.Plural && x.IsoCode == y.IsoCode && x.NumericCode == y.NumericCode && x.Name == y.Name;
    }

    public int GetHashCode(ICurrency obj)
    {
        return HashCode.Combine(obj.Symbol, obj.Singular, obj.Plural, obj.IsoCode, obj.NumericCode, obj.Name);
    }
}