using System;

using System.Runtime.CompilerServices;  // for MethodImplOptions
using System.Diagnostics; // for StackFrame
using System.IO; // for Path
using System.IO.Compression; // for ZipFile

using Microsoft.WindowsAzure.Storage; // for CloudStorageAccount

namespace zip
{
    class Program
    {
        enum LogType { info, warning, error };

        static int _log_verbosity = 5;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Log(LogType type, int level, string message, string detail)
        {
            if (_log_verbosity < level)
                return;

            string caller_name = new StackFrame(1, true).GetMethod().Name;
            DateTime time = DateTime.Now;
            Console.WriteLine($"{time.ToString()}.{time.Millisecond.ToString()},{caller_name},{type.ToString()},{level.ToString()},{message},{detail}");
        }

        static void Main(string[] args)
        {
            string command = args.Length > 0 ? args[0] : "help";
            try
            {

                if (command.Equals("push_file", StringComparison.InvariantCultureIgnoreCase))
                    PushFile(args[1], args[2], true);
                else if (command.Equals("pull_file", StringComparison.InvariantCultureIgnoreCase))
                    PullFile(args[1], args[2], true);

                if (!command.Equals("help", StringComparison.InvariantCultureIgnoreCase))
                    return;
            }
            catch (System.Exception ex)
            {
                Log(LogType.error, 1, $"Could not execute command {command}", $"Inner error: {ex.Message}");
            }

            //help
            Console.WriteLine("Commands:");
            Console.WriteLine("- push_file <file> <storage_connection_string>\t\tE.g.: \"push_file\" \"./README.md\" \"MyConnectionString\"");
            Console.WriteLine("- pull_file <source> <destination>\t\tE.g.: \"unzip\" \"./README.md\" \"./README.zip\"");
        }

        static private bool CanonizePath(string input, out string output)
        {
            try
            {
                output = Path.GetFullPath(input);
            }
            catch (System.Exception)
            {
                output = "";
                return false;
            }

            return true;
        }

        static private bool RemoveDirectory(string path)
        {
            try
            {
                Directory.Delete(path, true);
                return true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($" - Error: {ex.Message}");
                return false;
            }
        }
        
         static private bool RemoveFile(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($" - Error: {ex.Message}");
                return false;
            }
        }

        static bool PushFile(string file, string connection_string, bool replace)
        {
            try
            {
                Console.WriteLine($"Pusshing File to Azure Storage {file}:");

                string source_path, destination_path;
                if (!CanonizePath(file, out source_path))
                {
                    Console.WriteLine($" - Invalid parameters.");
                    return false;
                }

                
               CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
               

                //Console.WriteLine($" - {destination} created with success!");
                return true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($" - Error: {ex.Message}");
                return false;
            }
        }

        static private bool PullFile(string source, string destination, bool replace)
        {
            try
            {
                Console.WriteLine($"Unzipping {source}:");

                string source_path, destination_path;
                if (!CanonizePath(source, out source_path) || !CanonizePath(destination, out destination_path))
                {
                    Console.WriteLine($" - Invalid parameters.");
                    return false;
                }

                if (Directory.Exists(destination_path))
                {
                    if (!replace)
                    {
                        Console.WriteLine($" - Destination already exist.");
                        return false;
                    }

                    if (!RemoveDirectory(destination_path))
                    {
                        Console.WriteLine($" - Could not remove directory.");
                        return false;
                    }
                }

                ZipFile.ExtractToDirectory(source_path, destination_path);
                Console.WriteLine($" - {destination} created with success!");
                return true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($" - Error: {ex.Message}");
                return false;
            }
        }
    }
}
