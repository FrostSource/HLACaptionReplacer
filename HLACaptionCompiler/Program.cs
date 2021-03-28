using HLACaptionCompiler.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


#nullable enable

namespace HLACaptionCompiler
{
    public enum InputMode
    {
        Custom,
        Replace
    }

    class Program
    {
        // Currently defaults to true while testing.
        private static bool PauseOnCompletion { get; set; } = true;
        private static bool Verbose { get; set; } = false;
        private static InputMode InputMode { get; set; } = InputMode.Replace;
        private static IList<FileInfo> Files { get; set; } = new List<FileInfo>();
        public const string CaptionSourceFilePattern = "closecaption_*.txt";
        public const string CaptionCompiledFilePattern = "closecaption_*.dat";

        static void Main(string[] args)
        {
            //Console.WriteLine("Hello Caption Replacers!!");


            //Console.WriteLine($"Current dir: {Directory.GetCurrentDirectory()}");
            //Console.WriteLine($"Num args: {args.Length}");

            // Needs a proper parsing library if released
            ParseArgs(args);

            if (Files.Count == 0)
            {
                var workingDirectory = Directory.GetCurrentDirectory();
                var hlaMainAddonFolder = Steam.SteamData.GetHLAMainAddOnFolder();
                // Not inside an addon
                if (!hlaMainAddonFolder.Contains(workingDirectory) || hlaMainAddonFolder == workingDirectory)
                {
                    WriteVerbose("Not inside addon folder. Using list selection.");

                    var addon = GetUserAddonChoice();
                    if (addon == "") return;
                    var contentPath = Steam.SteamData.GetHLAAddOnFolder(addon);
                    var gamePath = Steam.SteamData.GetAddOnGameFolder(addon);
                    var contentCaptionDirectory = new DirectoryInfo(Path.Combine(contentPath, Steam.SteamData.CaptionFolder));
                    var gameCaptionDirectory = new DirectoryInfo(Path.Combine(gamePath, Steam.SteamData.CaptionFolder));

                    CompileFolder(contentCaptionDirectory, gameCaptionDirectory);
                }
                //var directories = working.Split(Path.DirectorySeparatorChar);
                //var info = new DirectoryInfo(working);
                //info.
                //foreach (var directory in directories)
                //{
                //    Console.WriteLine(directory);
                //}

            }

            if (PauseOnCompletion)
            {
                Console.WriteLine("\nPress any key to exit the process...");
                Console.ReadKey();
            }
        }

        private static void ParseArgs(string[] args)
        {
            foreach (var arg in args)
            {
                // Fully qualified options
                if (arg.StartsWith("--"))
                {
                    var option = arg[2..];
                    if (option != string.Empty)
                    {
                        SetOption(option);
                    }
                    continue;
                }
                // Single option(s)
                if (arg.StartsWith("-"))
                {
                    var options = arg[1..];
                    foreach (var option in options)
                    {
                        SetOption(option.ToString());
                    }
                    continue;
                }
                // Presumed files
                if (File.Exists(arg))
                {
                    try
                    {
                        Files.Add(new FileInfo(arg));
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine($"Not a valid filename: {arg}");
                    }
                    catch (PathTooLongException)
                    {
                        Console.WriteLine($"Path too long: {arg}");
                    }
                    catch (SystemException)
                    {
                        Console.WriteLine($"Program does not have access to file: {arg}");
                    }
                    continue;
                }

                Console.WriteLine($"Unknown file or option: {arg}");
            }
        }
        public static void SetOption(string option)
        {
            switch (option.ToLower())
            {
                case "pause":
                case "p":
                    PauseOnCompletion = true;
                    break;

                case "verbose":
                case "v":
                    Verbose = true;
                    break;
            }
        }
        public static void SetOptions(string[] options)
        {
            foreach (var option in options)
            {
                SetOption(option);
            }
        }
        private static string GetUserAddonChoice(string message = "Please choose an addon to compile captions for, or leave blank to exit:\n")
        {
            var addons = Steam.SteamData.GetAddOnList();

            Console.WriteLine(message);
            for (int i = 0; i < addons.Length; i++)
            {
                Console.WriteLine($"{i+1}. {addons[i]}");
            }
            Console.Write("> ");
            var input = Console.ReadKey();

            int selection = 1;
            while (input.Key != ConsoleKey.Enter && !int.TryParse(input.KeyChar.ToString(), out selection) && (selection < 1 || selection > addons.Length))
            {
                Console.WriteLine($"{input.KeyChar} is not a valid selection.");
                Console.Write("> ");
                input = Console.ReadKey();
            }

            if (input.Key == ConsoleKey.Enter) return "";

            return addons[selection];
            
        }
        public static void CompileFile(FileInfo file, DirectoryInfo dir)
        {
            var parser = new ClosedCaptionFileParser(file);
            if (parser.TryParse(out var parsed))
            {
                var captions = new ClosedCaptions();
                foreach (KeyValuePair<string,string> token in parsed["Tokens"])
                {
                    captions.Add(token.Key, token.Value);
                }
                captions.Write(Path.Combine(dir.FullName, file.Name, ".dat"));
            }
        }
        public static void CompileFolder(DirectoryInfo from, DirectoryInfo to)
        {
            var files = from.GetFiles(CaptionSourceFilePattern);
            foreach (var file in files)
            {
                CompileFile(file, to);
            }
        }

        private static void WriteVerbose(string message)
        {
            if (Verbose) Console.WriteLine(message);
        }

        public static void PrintHelp()
        {
            var help = "Command line arguments must contain an input file and a caption file.\n" +
                ".txt is recognized as an input file (either a custom language file or modifier file).\n" +
                ".dat is recognized as a compiled caption file (e.g. closecaption_english.dat).\n" +
                "\n" +
                "Flags:\n" +
                "-p     Wait for input after finishing\n";
            Console.WriteLine(help);
        }


    }

    
}
