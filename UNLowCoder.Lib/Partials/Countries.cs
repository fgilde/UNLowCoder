using UNLowCoder.Core.Data;

namespace UNLowCoder.Lib;

public partial class UnLocodes
{
    public static partial class Countries
    {
    }
    public static partial class Subdivisions
    {
        public static UnLocodeSubdivision Find(string subdivisionCode)
        {
            return All.FirstOrDefault(l => l.SubdivisionCode.Equals(subdivisionCode, StringComparison.OrdinalIgnoreCase))
                   ?? throw new ArgumentException($"Subdivision with code '{subdivisionCode}' not found.");
        }
    }
    public static partial class Locations
    {
        public static UnLocodeLocation Find(string fullLocode)
        {
            return All.FirstOrDefault(l => l.FullUnLocode.Equals(fullLocode, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException($"Location with full UnLocode '{fullLocode}' not found.");
        }
    }
}