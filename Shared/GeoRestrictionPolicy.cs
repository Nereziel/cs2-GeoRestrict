using System.Net;

namespace GeoRestrict.Shared;

public static class GeoRestrictionPolicy
{
    public static IPAddress? ParseIpAddress(string? rawAddress)
        => throw new NotImplementedException();

    public static bool IsAllowed(
        string? countryCode,
        IEnumerable<string>? configuredCountries,
        bool whitelist,
        bool allowFromUnknown)
        => throw new NotImplementedException();

    public static string SanitizeKickMessage(string? message)
        => throw new NotImplementedException();
}
