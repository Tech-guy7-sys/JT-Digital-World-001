using Xunit;
using System.Text;
using AgentJeff;

namespace AgentJeff.Tests;

public sealed class ThreatSignatureDatabaseTests
{
    private const string Eicar = @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";

    [Fact]
    public void DetectsHarmlessEicarTestString()
    {
        var db = new ThreatSignatureDatabase();
        var bytes = Encoding.ASCII.GetBytes(Eicar);

        Assert.True(db.IsEicarTestFile(bytes));
        Assert.Contains("EICAR", db.MatchContent(bytes));
    }

    [Fact]
    public void DoesNotMatchOrdinaryTextAsEicar()
    {
        var db = new ThreatSignatureDatabase();
        Assert.Null(db.MatchContent(Encoding.UTF8.GetBytes("hello world")));
    }

    [Fact]
    public void ComputesStableSha256()
    {
        var hash = ThreatSignatureDatabase.ComputeSha256(Encoding.ASCII.GetBytes("abc"));
        Assert.Equal("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", hash);
    }

    [Fact]
    public async Task FileScannerFlagsEicarTestFileWithoutExecutingIt()
    {
        var path = Path.Combine(Path.GetTempPath(), $"jt-eicar-test-{Guid.NewGuid():N}.txt");
        try
        {
            await File.WriteAllTextAsync(path, Eicar, Encoding.ASCII);
            var result = await new AntivirusEngine().ScanFileAsync(path);
            Assert.Equal(FileRisk.Suspicious, result.Risk);
            Assert.Contains("EICAR", result.Summary);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
