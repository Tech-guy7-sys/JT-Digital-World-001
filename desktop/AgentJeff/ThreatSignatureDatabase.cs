using System.Security.Cryptography;
using System.Text;

namespace AgentJeff;

/// <summary>
/// Small, transparent starter signature set. This is an extensible detection component,
/// not a comprehensive malware intelligence feed. Signatures are matched by exact SHA-256
/// or by the harmless industry-standard EICAR antivirus test string.
/// </summary>
public sealed class ThreatSignatureDatabase
{
    private const string EicarTestString = "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";

    private static readonly IReadOnlyDictionary<string, string> Sha256Signatures =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Add verified SHA-256 indicators from a trusted, authenticated source here.
            // Do not treat unsigned user-editable feeds as trusted detections.
        };

    public static string ComputeSha256(ReadOnlySpan<byte> data) =>
        Convert.ToHexString(SHA256.HashData(data));

    public string? MatchHash(string sha256) =>
        Sha256Signatures.TryGetValue(sha256, out var name) ? name : null;

    public bool IsEicarTestFile(ReadOnlySpan<byte> content)
    {
        var testBytes = Encoding.ASCII.GetBytes(EicarTestString);
        return content.SequenceEqual(testBytes);
    }

    public string? MatchContent(ReadOnlySpan<byte> content)
    {
        if (IsEicarTestFile(content))
            return "EICAR antivirus test file (harmless detection test; not a real virus)";
        return null;
    }
}
