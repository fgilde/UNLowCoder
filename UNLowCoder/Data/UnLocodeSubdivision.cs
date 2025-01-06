using Newtonsoft.Json;
using System;

namespace UNLowCoder.Core.Data;

public partial record UnLocodeSubdivision
{
    [JsonIgnore]
    public UnLocodeCountry Country => CountryResolverFunc?.Invoke();
    [JsonIgnore]
    public Func<UnLocodeCountry> CountryResolverFunc { get; set; }

    public string CountryCode { get; }
    public string SubdivisionCode { get; }
    public string Name { get; }
    public string Type { get; }

    public UnLocodeSubdivision()
    {}

    public UnLocodeSubdivision(string countryCode, string subdivisionCode, string name, string type)
    {
        CountryCode = countryCode;
        SubdivisionCode = subdivisionCode;
        Name = name;
        Type = type;
    }
}
