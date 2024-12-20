using System;

namespace UNLowCoder.Core.Data;

[Flags]
public enum LocationFunction
{
    None = 0,
    Seaport = 1 << 0,     // Position 1 == '1'?
    RailTerminal = 1 << 1, // Position 2 == '2'
    RoadTerminal = 1 << 2, // Position 3 == '3'
    Airport = 1 << 3,      // Position 4 == '4'
    PostOffice = 1 << 4,   // Position 5 == '5'
    ICD = 1 << 5,          // Position 6 == '6'
    FixedTransport = 1 << 6 // Position 7 == '7'
}