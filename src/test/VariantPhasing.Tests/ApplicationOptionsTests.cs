using System.IO;
using Xunit;


namespace VariantPhasing.Tests
{
    public class ApplicationOptionsTests
    {
        [Fact]
        public void LogFolder()
        {
            var options = new ScyllaApplicationOptions();
            options.OutputDirectory = "VariantPhasingTestOut";
            options.SetIODirectories("Scylla");
            Assert.Equal(Path.Combine("VariantPhasingTestOut","ScyllaLogs"), options.LogFolder);
        }
    }
}
