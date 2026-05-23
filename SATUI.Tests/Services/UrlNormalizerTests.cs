using SATUI.Services;

namespace SATUI.Tests.Services;

public class UrlNormalizerTests
{
    // ── Validate ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WhenEmpty_ReturnsError()
        => UrlNormalizer.Validate("").ShouldNotBeNull();

    [Fact]
    public void Validate_WhenWhitespaceOnly_ReturnsError()
        => UrlNormalizer.Validate("   ").ShouldNotBeNull();

    [Fact]
    public void Validate_WhenValidIPv4_ReturnsNull()
        => UrlNormalizer.Validate("192.168.1.100").ShouldBeNull();

    [Fact]
    public void Validate_WhenValidIPv4WithPort_ReturnsNull()
        => UrlNormalizer.Validate("192.168.1.100:8080").ShouldBeNull();

    [Fact]
    public void Validate_WhenIPv4WithInvalidOctet_ReturnsError()
        => UrlNormalizer.Validate("300.168.1.100").ShouldNotBeNull();

    [Fact]
    public void Validate_WhenValidHostname_ReturnsNull()
        => UrlNormalizer.Validate("sat-terminal").ShouldBeNull();

    [Fact]
    public void Validate_WhenMdnsHostname_ReturnsNull()
        => UrlNormalizer.Validate("device.local").ShouldBeNull();

    [Fact]
    public void Validate_WhenHostnameWithPort_ReturnsNull()
        => UrlNormalizer.Validate("device.local:8080").ShouldBeNull();

    [Fact]
    public void Validate_WhenFullHttpsUrl_ReturnsNull()
        => UrlNormalizer.Validate("https://192.168.1.100").ShouldBeNull();

    [Fact]
    public void Validate_WhenFullHttpUrl_ReturnsNull()
        => UrlNormalizer.Validate("http://sat-terminal/ui").ShouldBeNull();

    [Fact]
    public void Validate_WhenIPWithPath_ReturnsNull()
        => UrlNormalizer.Validate("192.168.1.100/dashboard").ShouldBeNull();

    [Fact]
    public void Validate_WhenFtpUrl_ReturnsError()
        => UrlNormalizer.Validate("ftp://device.local").ShouldNotBeNull();

    [Fact]
    public void Validate_WhenInputContainsSpaces_ReturnsError()
        => UrlNormalizer.Validate("192.168 .1.100").ShouldNotBeNull();

    [Fact]
    public void Validate_WhenGarbageInput_ReturnsError()
        => UrlNormalizer.Validate("@#$%!").ShouldNotBeNull();

    // ── GetHint ─────────────────────────────────────────────────────────────

    [Fact]
    public void GetHint_WhenBareIP_ReturnsHintText()
        => UrlNormalizer.GetHint("192.168.1.100").ShouldNotBeNull();

    [Fact]
    public void GetHint_WhenHostname_ReturnsHintText()
        => UrlNormalizer.GetHint("device.local").ShouldNotBeNull();

    [Fact]
    public void GetHint_WhenIPWithPort_ReturnsHintText()
        => UrlNormalizer.GetHint("192.168.1.100:8080").ShouldNotBeNull();

    [Fact]
    public void GetHint_WhenHttpsUrl_ReturnsNull()
        => UrlNormalizer.GetHint("https://192.168.1.100").ShouldBeNull();

    [Fact]
    public void GetHint_WhenHttpUrl_ReturnsNull()
        => UrlNormalizer.GetHint("http://192.168.1.100").ShouldBeNull();

    [Fact]
    public void GetHint_WhenEmpty_ReturnsNull()
        => UrlNormalizer.GetHint("").ShouldBeNull();

    // ── GetCandidates ────────────────────────────────────────────────────────

    [Fact]
    public void GetCandidates_WhenBareIP_ReturnsBothProtocolsHttpsFirst()
    {
        var result = UrlNormalizer.GetCandidates("192.168.1.100");

        result.ShouldBe(new[] { "https://192.168.1.100", "http://192.168.1.100" });
    }

    [Fact]
    public void GetCandidates_WhenIPWithPort_ReturnsBothProtocolsWithPort()
    {
        var result = UrlNormalizer.GetCandidates("192.168.1.100:8080");

        result.ShouldBe(new[] { "https://192.168.1.100:8080", "http://192.168.1.100:8080" });
    }

    [Fact]
    public void GetCandidates_WhenHostname_ReturnsBothProtocols()
    {
        var result = UrlNormalizer.GetCandidates("device.local");

        result.ShouldBe(new[] { "https://device.local", "http://device.local" });
    }

    [Fact]
    public void GetCandidates_WhenHttpsUrl_ReturnsHttpsFirst()
    {
        var result = UrlNormalizer.GetCandidates("https://192.168.1.100");

        result[0].ShouldStartWith("https://");
        result.Count.ShouldBe(2);
    }

    [Fact]
    public void GetCandidates_WhenHttpUrl_ReturnsHttpFirst()
    {
        var result = UrlNormalizer.GetCandidates("http://192.168.1.100");

        result[0].ShouldStartWith("http://");
        result.Count.ShouldBe(2);
    }

    [Fact]
    public void GetCandidates_WhenIPWithPath_ReturnsBothWithPath()
    {
        var result = UrlNormalizer.GetCandidates("192.168.1.100/dashboard");

        result.ShouldBe(new[] { "https://192.168.1.100/dashboard", "http://192.168.1.100/dashboard" });
    }

    [Fact]
    public void GetCandidates_WhenEmpty_ReturnsEmpty()
        => UrlNormalizer.GetCandidates("").ShouldBeEmpty();

    [Fact]
    public void GetCandidates_WhenInvalidInput_ReturnsEmpty()
        => UrlNormalizer.GetCandidates("@#$%").ShouldBeEmpty();
}
