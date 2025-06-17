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

        public static IEnumerable<UnLocodeSubdivision> FindAll(string subdivisionCode)
        {
            return All.Where(l => l.SubdivisionCode.Equals(subdivisionCode, StringComparison.OrdinalIgnoreCase));
        }
    }
    public static partial class Locations
    {
    
        public static UnLocodeLocation? Find(string fullLocode)
        {
            return All.FirstOrDefault(l => l.FullUnLocode.Equals(fullLocode, StringComparison.OrdinalIgnoreCase));
        }

        public static UnLocodeLocation? Find(double latitude, double longitude) => Find(new Coordinates(latitude, longitude));
        public static UnLocodeLocation? Find(double latitude, double longitude, double minDistance) => Find(new Coordinates(latitude, longitude), minDistance);

        public static UnLocodeLocation? Find(Coordinates coordinates)
        {
            const double tolerance = 1e-6;
            return All.FirstOrDefault(l =>
                l.Coordinates != null
                && Math.Abs(l.Coordinates.Latitude - coordinates.Latitude) < tolerance
                && Math.Abs(l.Coordinates.Longitude - coordinates.Longitude) < tolerance
            );
        }
        
        public static UnLocodeLocation? Find(Coordinates coordinates, double minDistance)
        {
            return All.FirstOrDefault(l => l.Coordinates?.DistanceTo(coordinates) <= minDistance);
        }
        

        public static IEnumerable<UnLocodeLocation> FindAll(double latitude, double longitude) => FindAll(new Coordinates(latitude, longitude));
        public static IEnumerable<UnLocodeLocation> FindAll(double latitude, double longitude, double minDistance) => FindAll(new Coordinates(latitude, longitude), minDistance);

        public static IEnumerable<UnLocodeLocation> FindAll(Coordinates coordinates)
        {
            const double tolerance = 1e-6;
            return All.Where(l =>
                l.Coordinates != null
                && Math.Abs(l.Coordinates.Latitude - coordinates.Latitude) < tolerance
                && Math.Abs(l.Coordinates.Longitude - coordinates.Longitude) < tolerance
            );
        }

        public static IEnumerable<UnLocodeLocation> FindAll(Coordinates coordinates, double minDistance)
        {
            return All.Where(l => l.Coordinates?.DistanceTo(coordinates) <= minDistance);
        }
    }
}