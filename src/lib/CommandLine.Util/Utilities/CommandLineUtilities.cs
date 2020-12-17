using System;
using System.Collections.Generic;
using Common.IO;

namespace CommandLine.Util
{
    public static class CommandLineUtilities
    {


        /// <summary>
        /// Displays the command-line banner for this program
        /// </summary>
        public static void DisplayBanner(string author)
        {
            // create the top and bottom lines
            const int lineLength = 75;
            var line = new string('-', lineLength);

            // create the filler string
            //var appInfo = Common.IO.PiscesSuiteAppInfo;
            bool loaded = Common.IO.PiscesSuiteAppInfo.TestLoaded();
            //if (loaded)
            //    Console.WriteLine("loaded!");

            string t = Common.IO.PiscesSuiteAppInfo.Title;
            string cr = Common.IO.PiscesSuiteAppInfo.Copyright;

            int fillerLength = lineLength - Common.IO.PiscesSuiteAppInfo.Title.Length - Common.IO.PiscesSuiteAppInfo.Copyright.Length;
            int fillerLength2 = lineLength - author.Length - Common.IO.PiscesSuiteAppInfo.InformationalVersion.Length;

            if (fillerLength < 1)
            {
                throw new InvalidOperationException("Unable to display the program banner, the program name is too long.");
            }

            if (fillerLength2 < 1)
            {
                throw new InvalidOperationException("Unable to display the program banner, the author name and version string is too long.");
            }

            var filler = new string(' ', fillerLength);
            var filler2 = new string(' ', fillerLength2);

            // display the actual banner
            Console.WriteLine(line);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(PiscesSuiteAppInfo.Title);
            Console.ResetColor();
            Console.WriteLine("{0}{1}", filler, PiscesSuiteAppInfo.Copyright);
            Console.WriteLine("{0}{1}{2}", author, filler2, PiscesSuiteAppInfo.InformationalVersion);
            Console.WriteLine("{0}\n", line);
            Console.WriteLine("Please reference '{0}'\n", PiscesSuiteAppInfo.Reference);            
            Console.WriteLine("{0}\n", line);
        }

        /// <summary>
        /// Displays a list of the unsupported options encountered on the command line
        /// </summary>
        public static void ShowUnsupportedOptions(List<string> unsupportedOps)
        {
            if (unsupportedOps == null || unsupportedOps.Count == 0) return;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Unsupported options:");
            Console.ResetColor();

            var spacer = new string(' ', 3);
            foreach (var option in unsupportedOps)
            {
                Console.WriteLine(spacer + option);
            }
        }
    }
}


