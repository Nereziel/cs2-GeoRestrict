using System.Net;

namespace GeoRestrict.Shared;

public enum GeoRestrictionProvider
{
    Http,
    LocalDb
}

public static class GeoRestrictionPolicy
{
    public static GeoRestrictionProvider ParseProvider(string? value)
    {
        return string.Equals(value?.Trim(), "LocalDb", StringComparison.OrdinalIgnoreCase)
            ? GeoRestrictionProvider.LocalDb
            : GeoRestrictionProvider.Http;
    }

    public static IPAddress? ParseIpAddress(string? rawAddress)
    {
        if (string.IsNullOrWhiteSpace(rawAddress))
        {
            return null;
        }

        var value = rawAddress.Trim();

        if (IPAddress.TryParse(value, out var directAddress))
        {
            return directAddress;
        }

        if (value.StartsWith('['))
        {
            var closingBracket = value.IndexOf(']');
            if (closingBracket > 1 && IPAddress.TryParse(value[1..closingBracket], out var ipv6Address))
            {
                return ipv6Address;
            }
        }

        var lastColon = value.LastIndexOf(':');
        if (lastColon > 0 && value.IndexOf(':') == lastColon &&
            IPAddress.TryParse(value[..lastColon], out var ipv4Address))
        {
            return ipv4Address;
        }

        return null;
    }

    public static bool IsAllowed(
        string? countryCode,
        IEnumerable<string>? configuredCountries,
        bool whitelist,
        bool allowFromUnknown)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return allowFromUnknown;
        }

        var countries = configuredCountries ?? [];
        var isConfigured = countries.Any(code =>
            string.Equals(code?.Trim(), countryCode.Trim(), StringComparison.OrdinalIgnoreCase));

        return whitelist ? isConfigured : !isConfigured;
    }

    public static string SanitizeKickMessage(string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return string.Empty;
        }

        return message
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal)
            .Replace("\"", string.Empty, StringComparison.Ordinal)
            .Replace(";", string.Empty, StringComparison.Ordinal);
    }
}
