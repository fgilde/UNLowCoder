using System;
using System.Collections.Generic;
using System.Linq;
using Nager.Country;

namespace UNLowCoder.Extensions
{
    public class CountryEqualityComparer : IEqualityComparer<ICountryInfo>
    {
        public static CountryEqualityComparer Instance { get; } = new();

        public bool Equals(ICountryInfo x, ICountryInfo y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            if (x.GetType() != y.GetType()) return false;

            return x.CommonName == y.CommonName &&
                   x.OfficialName == y.OfficialName &&
                   x.NativeName == y.NativeName &&
                   x.Alpha2Code == y.Alpha2Code &&
                   x.Alpha3Code == y.Alpha3Code &&
                   x.NumericCode == y.NumericCode &&
                   x.TLD.SequenceEqual(y.TLD) &&
                   x.Region == y.Region &&
                   x.SubRegion == y.SubRegion &&
                   x.BorderCountries.SequenceEqual(y.BorderCountries) &&
                   x.Currencies.SequenceEqual(y.Currencies, NagerCurrencyComparer.Instance) &&
                   x.CallingCodes.SequenceEqual(y.CallingCodes);
        }

        public int GetHashCode(ICountryInfo obj)
        {
            if (obj == null) return 0;

            unchecked // overflow ok
            {
                int hash = 17;
                hash = hash * 23 + (obj.CommonName?.GetHashCode() ?? 0);
                hash = hash * 23 + (obj.OfficialName?.GetHashCode() ?? 0);
                hash = hash * 23 + (obj.NativeName?.GetHashCode() ?? 0);
                hash = hash * 23 + obj.Alpha2Code.GetHashCode();
                hash = hash * 23 + obj.Alpha3Code.GetHashCode();
                hash = hash * 23 + obj.NumericCode.GetHashCode();
                hash = hash * 23 + (obj.TLD != null ? GetEnumerableHashCode(obj.TLD) : 0);
                hash = hash * 23 + obj.Region.GetHashCode();
                hash = hash * 23 + obj.SubRegion.GetHashCode();
                hash = hash * 23 + (obj.BorderCountries != null ? GetEnumerableHashCode(obj.BorderCountries) : 0);
                hash = hash * 23 + (obj.Currencies != null ? GetEnumerableHashCode(obj.Currencies, NagerCurrencyComparer.Instance) : 0);
                hash = hash * 23 + (obj.CallingCodes != null ? GetEnumerableHashCode(obj.CallingCodes) : 0);
                return hash;
            }
        }

        private static int GetEnumerableHashCode<T>(IEnumerable<T> enumerable, IEqualityComparer<T> comparer = null)
        {
            unchecked
            {
                int hash = 19;
                foreach (var item in enumerable)
                {
                    hash = hash * 31 + (item == null ? 0 : (comparer?.GetHashCode(item) ?? item.GetHashCode()));
                }
                return hash;
            }
        }
    }

    public class NagerCurrencyComparer : IEqualityComparer<ICurrency>
    {
        public static NagerCurrencyComparer Instance { get; } = new();

        public bool Equals(ICurrency x, ICurrency y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            if (x.GetType() != y.GetType()) return false;

            return x.Symbol == y.Symbol &&
                   x.Singular == y.Singular &&
                   x.Plural == y.Plural &&
                   x.IsoCode == y.IsoCode &&
                   x.NumericCode == y.NumericCode &&
                   x.Name == y.Name;
        }

        public int GetHashCode(ICurrency obj)
        {
            if (obj == null) return 0;

            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (obj.Symbol?.GetHashCode() ?? 0);
                hash = hash * 23 + (obj.Singular?.GetHashCode() ?? 0);
                hash = hash * 23 + (obj.Plural?.GetHashCode() ?? 0);
                hash = hash * 23 + (obj.IsoCode?.GetHashCode() ?? 0);
                hash = hash * 23 + obj.NumericCode.GetHashCode();
                hash = hash * 23 + (obj.Name?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
