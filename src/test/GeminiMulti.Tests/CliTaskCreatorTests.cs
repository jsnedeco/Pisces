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
            
            string expected_exe_string =  OS_SpecificString ;
            string actual_exe_string = task.ExecutablePath;
            Assert.Equal(expected_exe_string,actual_exe_string ); 


            string expected_arg_string =  "--args1 1thing -args2 another --chromRefId \"1\" --outFolder \"Outdir\"";
            string actual_arg_string = task.CommandLineArguments;
            Assert.Equal(expected_arg_string,actual_arg_string );    
        }
    }
}