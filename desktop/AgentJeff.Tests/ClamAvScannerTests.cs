using AgentJeff;
using Xunit;

namespace AgentJeff.Tests;

public sealed class ClamAvScannerTests
{
    [Theory]
    [InlineData(0, ClamAvScanStatus.Clean)]
    [InlineData(1, ClamAvScanStatus.ThreatFound)]
    [InlineData(2, ClamAvScanStatus.EngineError)]
    [InlineData(127, ClamAvScanStatus.EngineError)]
    [InlineData(-1, ClamAvScanStatus.EngineError)]
    public void MapExitCodeUsesClamAvExitCodeContract(int exitCode, ClamAvScanStatus expected)
    {
        Assert.Equal(expected, ClamAvScanner.MapExitCode(exitCode));
    }
}
