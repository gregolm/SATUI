using System.Text.RegularExpressions;

namespace SATUI.Services;

/// <summary>
/// Validates and normalizes user-supplied URLs that may be bare IP addresses,
/// hostnames, or full http/https URLs. Generates an ordered list of candidate
/// URLs to probe (https before http unless http was explicitly supplied).
/// </summary>
public static class UrlNormalizer
{
    private static readonly string HintText =
        "Protocol not required — will try https then http automatically.";

    /// <summary>
    /// Returns <c>null</c> if <paramref name="input"/> is acceptable; otherwise
    /// returns a user-friendly validation error message.
    /// </summary>
    public static string? Validate(string input)
    {
        var trimmed = input?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
            return "Enter an IP address or hostname (e.g. 192.168.1.100)";

        var parsed = Parse(trimmed);
        if (!parsed.IsValid)
            return "Enter a valid IP address or hostname";

        return null;
    }

    /// <summary>
    /// Returns a helpful advisory string when the input contains no explicit
    /// protocol; <c>null</c> when the input already has <c>http://</c> or
    /// <c>https://</c>, or is empty/invalid.
    /// </summary>
    public static string? GetHint(string input)
    {
        var trimmed = input?.Trim() ?? string.Empty;
        if (trimmed.Length == 0) return null;

        if (trimmed.Contains("://", StringComparison.OrdinalIgnoreCase)) return null;

        var parsed = Parse(trimmed);
        return parsed.IsValid ? HintText : null;
    }

    /// <summary>
    /// Returns an ordered list of absolute URLs to try when connecting.
    /// The preferred protocol (https unless http was explicit) comes first.
    /// Returns an empty list when <paramref name="input"/> is empty or invalid.
    /// </summary>
    public static IReadOnlyList<string> GetCandidates(string input)
    {
        var trimmed = input?.Trim() ?? string.Empty;
        if (trimmed.Length == 0) return [];

        var (host, port, path, scheme, isValid) = Parse(trimmed);
        if (!isValid) return [];

        var hostAndPort = port.HasValue ? $"{host}:{port}" : host;
        var pathSuffix = string.IsNullOrEmpty(path) ? string.Empty : path;

        string Primary(string p) => $"{p}://{hostAndPort}{pathSuffix}";

        bool httpsFirst = !string.Equals(scheme, "http", StringComparison.OrdinalIgnoreCase);
        return httpsFirst
            ? [Primary("https"), Primary("http")]
            : [Primary("http"), Primary("https")];
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static (string Host, int? Port, string Path, string? Scheme, bool IsValid)
        Parse(string trimmed)
    {
        string? scheme = null;
        string toParse;

        if (trimmed.Contains("://", StringComparison.OrdinalIgnoreCase))
        {
            // User supplied an explicit scheme — validate it
            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri))
                return ("", null, "", null, false);

            if (!absoluteUri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
                !absoluteUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                return ("", null, "", null, false);

            scheme = absoluteUri.Scheme;
            var host = absoluteUri.Host;
            if (!IsValidHost(host)) return ("", null, "", null, false);

            int? port = absoluteUri.IsDefaultPort ? null : absoluteUri.Port;
            string path = absoluteUri.AbsolutePath == "/" ? "" : absoluteUri.AbsolutePath;
            return (host, port, path, scheme, true);
        }

        // No scheme — synthesise one so Uri can parse the rest
        toParse = "https://" + trimmed;
        if (!Uri.TryCreate(toParse, UriKind.Absolute, out var syntheticUri))
            return ("", null, "", null, false);

        var parsedHost = syntheticUri.Host;
        if (!IsValidHost(parsedHost)) return ("", null, "", null, false);

        int? parsedPort = syntheticUri.IsDefaultPort ? null : syntheticUri.Port;
        string parsedPath = syntheticUri.AbsolutePath == "/" ? "" : syntheticUri.AbsolutePath;
        return (parsedHost, parsedPort, parsedPath, null, true);
    }

    private static bool IsValidHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host)) return false;

        // If it looks like an IPv4 address (4 dot-separated all-numeric parts),
        // validate strictly as IP — don't fall back to hostname rules.
        var parts = host.Split('.');
        if (parts.Length == 4 && parts.All(p => p.Length > 0 && p.All(char.IsDigit)))
            return TryParseIPv4(host);

        return IsValidHostname(host);
    }

    private static bool TryParseIPv4(string host)
    {
        // Must be exactly four decimal octets, each 0-255
        var parts = host.Split('.');
        if (parts.Length != 4) return false;
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var octet)) return false;
            if (octet < 0 || octet > 255) return false;
            // Reject leading zeros (e.g. "01")
            if (part != octet.ToString()) return false;
        }
        return true;
    }

    private static bool IsValidHostname(string host)
    {
        if (host.Length > 253) return false;
        var labels = host.Split('.');
        foreach (var label in labels)
        {
            if (label.Length == 0 || label.Length > 63) return false;
            if (label.StartsWith('-') || label.EndsWith('-')) return false;
            if (!label.All(c => char.IsLetterOrDigit(c) || c == '-')) return false;
        }
        return true;
    }
}
