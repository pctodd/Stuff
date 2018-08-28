using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace HDFSUtil
{
    class HDFSUtil
    {
        static void Main(string[] args)
        {
            int selection = 99;
            do
            {
                selection = DisplayMenu();

                if (selection == 1) AddDirectory();
                if (selection == 2) PutFile();
                if (selection > 2) {Console.WriteLine("Not Implemented Yet, Press Any Key..."); Console.ReadKey();}
            } while (selection != 0);
        }

        static public int DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine(" HDFS Developer Operations");
            Console.WriteLine();
            Console.WriteLine("     1) Add Directory");
            Console.WriteLine("     2) Upload File");
            Console.WriteLine("     3) Remove Directory");
            Console.WriteLine("     4) Delete File");
            Console.WriteLine("     5) Rename Directory");
            Console.WriteLine("     6) Rename File");
            Console.WriteLine("     6) Move File");
            Console.WriteLine("     7) List Directory");
            Console.WriteLine("     0) Exit");
            Console.WriteLine();
            Console.Write("     Enter Selection: ");
            var result = Console.ReadLine();
            return Convert.ToInt32(result);
        }

        static void PutFile()
        {
            // Variables
            // TO-DO: Put these into a config for easy user-specific defaults
            string srcFileName = @"d:\zodiac\data\dummy.txt";
            string destFileName = "/nrrebi/datastaging/spark/dummy.txt";

            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("What is the source file's full path and filename?");
            Console.WriteLine("Press Enter to use default of " + srcFileName + "\n");

            string inputSrcFileName = Console.ReadLine();
            if (inputSrcFileName != "")
            {
                srcFileName = inputSrcFileName;
            }

            // TO-DO: Put this in a while loop
            if (!File.Exists(srcFileName))
            {
                Console.WriteLine("Filename invalid or file does not exist.");
            }

            Console.WriteLine("What is the target file's full path and filename?");
            Console.WriteLine("Press Enter to use default of " + destFileName + "\n");

            string inputDestFileName = Console.ReadLine();
            if (inputDestFileName != "")
            {
                destFileName = inputDestFileName;
            }

            // HDFS put doesn't allow embedded spaces in filenames, prefers %20 for space instead
            Regex regexEmbeddedSpaces = new Regex(@"\s+"); // One or more embedded space
            Match embeddedSpaces = regexEmbeddedSpaces.Match(srcFileName);
            if (embeddedSpaces.Success)
            {
                destFileName = Regex.Replace(destFileName, @"\s", "%20");
            }

            // Get directory path, make sure it exists
            string fullHDFSPath = destFileName;
            fullHDFSPath = Path.GetDirectoryName(destFileName);

            // TO-DO: Put this in a while loop
            // Allow to create directory
            if (! CheckDirectory(fullHDFSPath))
            {
                Console.WriteLine("Directory name invalid or directory does not exist.");
            }

            // TO-DO: Check if file already exists in target, overwrite or not?

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.FileName = "cmd.exe";
            psi.Verb = "runas";
            psi.Arguments = "/C hdfs dfs -put " + srcFileName + " " + destFileName;
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
        }

        static Boolean CheckDirectory(string fullHDFSPath)
        {
            // Check for Directory
            // This works but seems like would be better if we could get -test to work instead of -count
            // I couldn't get the errorlevel from -test to indicate found or not found
            Boolean dirFound = false;

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.FileName = "cmd.exe";
            psi.Arguments = "/C hdfs dfs -count " + fullHDFSPath;
            psi.Verb = "runas";
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
            StreamReader stdErrStrm = p.StandardError;
            string stdErr = stdErrStrm.ReadLine();

            if (string.IsNullOrEmpty(stdErr))
            {
                dirFound = true;
            }

            return dirFound;
        }

        static void AddDirectory()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("What is the full directory you want to add?");
            string fullHDFSPath = Console.ReadLine();

            // Get directory path, make sure it exists
            string parentDirectory = fullHDFSPath;

            List<String> directoriesToCreate = new List<String>();
            while (parentDirectory != @"\" && CheckDirectory(parentDirectory) == false)
            {
                if (CheckDirectory(parentDirectory) == false)
                {
                    directoriesToCreate.Add(parentDirectory.Replace(@"\","/"));
                }
                parentDirectory = Path.GetDirectoryName(parentDirectory);
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.FileName = "cmd.exe";
            psi.Verb = "runas";
            Process p;

            directoriesToCreate.Reverse();
            foreach (String directory in directoriesToCreate)
            {
                psi.Arguments = "/C hdfs dfs -mkdir " + @directory;
                p = new Process();
                p.StartInfo = psi;
                p.Start();
                p.WaitForExit();
                StreamReader stdErrStrm = p.StandardError;
                string stdErr = stdErrStrm.ReadLine();
                if (! string.IsNullOrEmpty(stdErr))
                {
                    Console.WriteLine("Error Creating Directory: " + stdErr);
                }
            }
        }
    }
}
