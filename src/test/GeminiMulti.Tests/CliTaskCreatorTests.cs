using System.IO;
using Xunit;

namespace GeminiMulti.Tests
{
    public class CliTaskCreatorTests
    {
        [Fact]
        public void GetCliTask()
        {
            var creator = new CliTaskCreator();
            var task = creator.GetCliTask(new[] {"--args1", "1thing", "-args2", "another"}, "chr1", Path.Combine("path", "with spaces","myexe"),
                "Outdir", 1);

            string OS_SpecificString=Path.Combine("path", "with spaces","myexe");
            string expected_string =  "\"" + OS_SpecificString + "\" --args1 1thing -args2 another --chromRefId \"1\" --outFolder \"Outdir\"";
            string actual_string = task.CommandLineArguments;
            Assert.Equal(expected_string,actual_string );    
        }
    }
}