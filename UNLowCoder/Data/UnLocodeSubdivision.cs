namespace UNLowCoder.Core.Data;

public class UnLocodeSubdivision
{
    public string CountryCode { get; }
    public string SubdivisionCode { get; }
    public string Name { get; }
    public string Type { get; }

    public UnLocodeSubdivision(string countryCode, string subdivisionCode, string name, string type)
    {
        CountryCode = countryCode;
        SubdivisionCode = subdivisionCode;
        Name = name;
        Type = type;
    }
}
