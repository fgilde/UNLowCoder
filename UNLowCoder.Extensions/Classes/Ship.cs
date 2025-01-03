using Itinero.Attributes;
using Itinero.Profiles;

namespace UNLowCoder.Extensions.Classes;

using System.Collections.Generic;

public class Ship : Vehicle, IProfileInstance
{
    public static readonly Ship Instance = new Ship();

    public override string Name => nameof(Ship);

    private static readonly HashSet<string> _shipWhiteList = new HashSet<string>
    {
        "waterway", // z.B. "river", "canal"
        "route",    // z.B. "ferry"
        "boat"      // boat=yes
    };
    public override HashSet<string> ProfileWhiteList => _shipWhiteList;

    // Hier könntest du noch andere Felder wie ProfileWhiteListCapable etc. definieren,
    // sofern du Weg-Tags blockieren willst.

    public override FactorAndSpeed FactorAndSpeed(
        IAttributeCollection attributes,
        Whitelist whitelist)
    {
        // Hier entscheidest du, ob ein Weg (bzw. Kante) befahrbar ist,
        // und wie schnell (km/h) oder langsam (CostFactor) du unterwegs bist.

        // Aus den Attributen kannst du z.B. ablesen:
        // - waterway=river? -> Speed=20 km/h
        // - route=ferry? -> Speed=15 km/h
        // - boat=yes => Speed=xx ?

        // Minimales Dummy-Beispiel:
        // "Erlaube" alles, was in meiner WhiteList enthalten ist, Speed=20
        // (1 / SpeedFactor = Zeit; Value = DistanceFactor)

        var result = new FactorAndSpeed
        {
            Direction = 0,   // 0 bedeutet befahrbar in beiden Richtungen
            SpeedFactor = 1 / 20.0f, // Speed 20 => Zeit pro km
            Value = 1 // Distanz-Kostenfaktor
        };

        return result;
    }

    // Hier gibst du z.B. an, ob "shortest" oder "fastest" - je nach Logik
    public Profile Profile => new Profile(Name, ProfileMetric.DistanceInMeters, new string[0], new Constraint[0], this);

    // Hier könntest du z.B. Constraints abbilden, falls du so etwas brauchst
    public float[] Constraints => null;
}
