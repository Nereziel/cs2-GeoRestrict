using GeoRestrict.Shared;

namespace GeoRestrict.Tests;

public sealed class GeoRestrictionPolicyTests
{
    [Theory]
    [InlineData("127.0.0.1:27015", "127.0.0.1")]
    [InlineData("203.0.113.10", "203.0.113.10")]
    [InlineData("[2001:db8::1]:27015", "2001:db8::1")]
    [InlineData("2001:db8::1", "2001:db8::1")]
    public void ParseIpAddress_HandlesIpv4AndIpv6(string input, string expected)
    {
        var result = GeoRestrictionPolicy.ParseIpAddress(input);
        Assert.NotNull(result);
        Assert.Equal(expected, result.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-ip")]
    public void ParseIpAddress_ReturnsNullForInvalidInput(string? input)
    {
        Assert.Null(GeoRestrictionPolicy.ParseIpAddress(input));
    }

    [Fact]
    public void IsAllowed_Whitelist_AllowsConfiguredCountryCaseInsensitively()
    {
        Assert.True(GeoRestrictionPolicy.IsAllowed("cz", ["CZ", "SK"], whitelist: true, allowFromUnknown: false));
    }

    [Fact]
    public void IsAllowed_Whitelist_RejectsUnconfiguredCountry()
    {
        Assert.False(GeoRestrictionPolicy.IsAllowed("DE", ["CZ", "SK"], whitelist: true, allowFromUnknown: false));
    }

    [Fact]
    public void IsAllowed_Blacklist_RejectsConfiguredCountry()
    {
        Assert.False(GeoRestrictionPolicy.IsAllowed("CZ", ["CZ", "SK"], whitelist: false, allowFromUnknown: true));
    }

    [Fact]
    public void IsAllowed_Unknown_RespectsConfiguration()
    {
        Assert.True(GeoRestrictionPolicy.IsAllowed(null, ["CZ"], whitelist: true, allowFromUnknown: true));
        Assert.False(GeoRestrictionPolicy.IsAllowed(null, ["CZ"], whitelist: true, allowFromUnknown: false));
    }

    [Fact]
    public void IsAllowed_UsesExactIsoCodes_NotSubstringMatches()
    {
        Assert.False(GeoRestrictionPolicy.IsAllowed("US", ["RUS"], whitelist: true, allowFromUnknown: false));
    }

    [Fact]
    public void SanitizeKickMessage_RemovesCommandSeparatorsAndControlCharacters()
    {
        Assert.Equal("No access  quitnow", GeoRestrictionPolicy.SanitizeKickMessage("No access \"; quit\nnow"));
    }
}
