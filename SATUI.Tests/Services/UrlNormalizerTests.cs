using FluentAssertions;
using SATUI.Services;

namespace SATUI.Tests.Services;

public class UrlNormalizerTests
{
    // ── Validate ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WhenEmpty_ReturnsError()
        => UrlNormalizer.Validate("").Should().NotBeNull();

    [Fact]
    public void Validate_WhenWhitespaceOnly_ReturnsError()
        => UrlNormalizer.Validate("   ").Should().NotBeNull();

    [Fact]
    public void Validate_WhenValidIPv4_ReturnsNull()
        => UrlNormalizer.Validate("192.168.1.100").Should().BeNull();

    [Fact]
    public void Validate_WhenValidIPv4WithPort_ReturnsNull()
        => UrlNormalizer.Validate("192.168.1.100:8080").Should().BeNull();

    [Fact]
    public void Validate_WhenIPv4WithInvalidOctet_ReturnsError()
        => UrlNormalizer.Validate("300.168.1.100").Should().NotBeNull();

    [Fact]
    public void Validate_WhenValidHostname_ReturnsNull()
        => UrlNormalizer.Validate("sat-terminal").Should().BeNull();

    [Fact]
    public void Validate_WhenMdnsHostname_ReturnsNull()
        => UrlNormalizer.Validate("device.local").Should().BeNull();

    [Fact]
    public void Validate_WhenHostnameWithPort_ReturnsNull()
        => UrlNormalizer.Validate("device.local:8080").Should().BeNull();

    [Fact]
    public void Validate_WhenFullHttpsUrl_ReturnsNull()
        => UrlNormalizer.Validate("https://192.168.1.100").Should().BeNull();

    [Fact]
    public void Validate_WhenFullHttpUrl_ReturnsNull()
        => UrlNormalizer.Validate("http://sat-terminal/ui").Should().BeNull();

    [Fact]
    public void Validate_WhenIPWithPath_ReturnsNull()
        => UrlNormalizer.Validate("192.168.1.100/dashboard").Should().BeNull();

    [Fact]
    public void Validate_WhenFtpUrl_ReturnsError()
        => UrlNormalizer.Validate("ftp://device.local").Should().NotBeNull();

    [Fact]
    public void Validate_WhenInputContainsSpaces_ReturnsError()
        => UrlNormalizer.Validate("192.168 .1.100").Should().NotBeNull();

    [Fact]
    public void Validate_WhenGarbageInput_ReturnsError()
        => UrlNormalizer.Validate("@#$%!").Should().NotBeNull();

    // ── GetHint ─────────────────────────────────────────────────────────────

    [Fact]
    public void GetHint_WhenBareIP_ReturnsHintText()
        => UrlNormalizer.GetHint("192.168.1.100").Should().NotBeNull();

    [Fact]
    public void GetHint_WhenHostname_ReturnsHintText()
        => UrlNormalizer.GetHint("device.local").Should().NotBeNull();

    [Fact]
    public void GetHint_WhenIPWithPort_ReturnsHintText()
        => UrlNormalizer.GetHint("192.168.1.100:8080").Should().NotBeNull();

    [Fact]
    public void GetHint_WhenHttpsUrl_ReturnsNull()
        => UrlNormalizer.GetHint("https://192.168.1.100").Should().BeNull();

    [Fact]
    public void GetHint_WhenHttpUrl_ReturnsNull()
        => UrlNormalizer.GetHint("http://192.168.1.100").Should().BeNull();

    [Fact]
    public void GetHint_WhenEmpty_ReturnsNull()
        => UrlNormalizer.GetHint("").Should().BeNull();

    // ── GetCandidates ────────────────────────────────────────────────────────

    [Fact]
    public void GetCandidates_WhenBareIP_ReturnsBothProtocolsHttpsFirst()
    {
        var result = UrlNormalizer.GetCandidates("192.168.1.100");

        result.Should().ContainInOrder("https://192.168.1.100", "http://192.168.1.100");
    }

    [Fact]
    public void GetCandidates_WhenIPWithPort_ReturnsBothProtocolsWithPort()
    {
        var result = UrlNormalizer.GetCandidates("192.168.1.100:8080");

        result.Should().ContainInOrder(
            "https://192.168.1.100:8080", "http://192.168.1.100:8080");
    }

    [Fact]
    public void GetCandidates_WhenHostname_ReturnsBothProtocols()
    {
        var result = UrlNormalizer.GetCandidates("device.local");

        result.Should().ContainInOrder("https://device.local", "http://device.local");
    }

    [Fact]
    public void GetCandidates_WhenHttpsUrl_ReturnsHttpsFirst()
    {
        var result = UrlNormalizer.GetCandidates("https://192.168.1.100");

        result[0].Should().StartWith("https://");
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetCandidates_WhenHttpUrl_ReturnsHttpFirst()
    {
        var result = UrlNormalizer.GetCandidates("http://192.168.1.100");

        result[0].Should().StartWith("http://");
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetCandidates_WhenIPWithPath_ReturnsBothWithPath()
    {
        var result = UrlNormalizer.GetCandidates("192.168.1.100/dashboard");

        result.Should().ContainInOrder(
            "https://192.168.1.100/dashboard", "http://192.168.1.100/dashboard");
    }

    [Fact]
    public void GetCandidates_WhenEmpty_ReturnsEmpty()
        => UrlNormalizer.GetCandidates("").Should().BeEmpty();

    [Fact]
    public void GetCandidates_WhenInvalidInput_ReturnsEmpty()
        => UrlNormalizer.GetCandidates("@#$%").Should().BeEmpty();
}
