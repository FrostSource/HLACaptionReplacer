using HLACaptionCompiler.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


#nullable enable

namespace HLACaptionCompiler
{
    class Program
    {
        private static CompilerSettings Settings { get; set; } = new CompilerSettings();
        private static bool WriteSettingsFile { get; set; } = false;
        private static IList<FileInfo> Files { get; set; } = new List<FileInfo>();
        public const string CaptionSourceFilePattern = "closecaption_*.txt";
        public const string CaptionCompiledFilePattern = "closecaption_*.dat";
        public static DirectoryInfo WorkingDirectory { get; private set; } = new DirectoryInfo(Directory.GetCurrentDirectory());
        public static string SettingsPath { get; set; } = Path.Combine(WorkingDirectory.FullName, "CompilerSettings.json");

        static void Main(string[] args)
        {
            ParseArgs(args);

            if (WriteSettingsFile)
            {
                WriteSettings();
            }
            else if (File.Exists(SettingsPath))
            {
                ReadSettings();
            }
            
            if (Files.Count == 0)
            {
                WriteLineVerbose("[Mode] Compiling from current addon folder or chosen addon.");
                CompileWorkingOrChosenDirectory();
            }
            else
            {
                WriteLineVerbose("[Mode] Compiling files given to command line.");
                foreach (var file in Files)
                {
                    CompileFile(file, WorkingDirectory);
                }
            }

            if (Settings.PauseOnCompletion)
            {
                Console.WriteLine("\nPress any key to exit the process...");
                Console.ReadKey();
            }
        }

        public static void CompileWorkingOrChosenDirectory()
        {
            var hlaMainAddonFolder = Steam.SteamData.GetHLAMainAddOnFolder();
            // Check if inside addon folder
            string addon = GetParentAddonName(WorkingDirectory.FullName);
            // Not inside an addon
            if (addon == "")
            {
                WriteLineVerbose("Not inside addon folder. Using list selection.");

                addon = GetUserAddonChoice();
                if (addon == "") return;
            }
            else
            {
                WriteLineVerbose($"Compiling captions for addon \"{addon}\".");
            }

            var contentPath = Steam.SteamData.GetHLAAddOnFolder(addon);
            var gamePath = Steam.SteamData.GetAddOnGameFolder(addon);
            var contentCaptionDirectory = new DirectoryInfo(Path.Combine(contentPath, Steam.SteamData.CaptionFolder));
            var gameCaptionDirectory = new DirectoryInfo(Path.Combine(gamePath, Steam.SteamData.CaptionFolder));
            var (filesCompiled, filesTotal) = CompileFolder(contentCaptionDirectory, gameCaptionDirectory);
            if (filesCompiled > 0)
                Console.WriteLine($"{filesCompiled}/{filesTotal} files compiled to \"{gameCaptionDirectory.FullName}\"");
            //else
            //    Console.WriteLine($"Failed to write captions.");
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
                    Settings.PauseOnCompletion = true;
                    break;

                case "verbose":
                case "v":
                    Settings.Verbose = true;
                    break;

                case "settings":
                case "s":
                    WriteSettingsFile = true;
                    break;

                default:
                    Console.WriteLine($"Unknown option '{option}'");
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
                Console.WriteLine(" is not a valid selection.");
                Console.Write("> ");
                input = Console.ReadKey();
            }
            Console.WriteLine();

            if (input.Key == ConsoleKey.Enter) return "";
            return addons[selection-1];
            
        }
        public static bool CompileFile(FileInfo file, DirectoryInfo dir)
        {
            WriteVerbose($"Compiling {file.Name} ... ");
            var parser = new ClosedCaptionFileParser(file);
            if (parser.TryParse(out var parsed))
            {
                var captions = new ClosedCaptions();
                foreach (KeyValuePair<string,string> token in parsed["Tokens"])
                {
                    captions.Add(token.Key, token.Value);
                }
                captions.Write(Path.Combine(dir.FullName, Path.ChangeExtension(file.Name, ".dat")));
                WriteLineVerbose("DONE.");
                return true;
            }
            Console.WriteLine($"{file.Name} failed to compile...");
            return false;
        }
        public static (int filesCompiled, int filesTotal) CompileFolder(DirectoryInfo from, DirectoryInfo to)
        {
            if (!from.Exists)
            {
                Console.WriteLine($"\"{Steam.SteamData.CaptionFolder}\" does not not exist in addon \"{GetParentAddonName(from.FullName)}\".");
                return (0,0);
            }
            int successes = 0;
            var files = from.GetFiles(CaptionSourceFilePattern);
            if (files.Length <= 0)
            {
                Console.WriteLine($"No caption content files found in addon \"{GetParentAddonName(from.FullName)}\".");
            }
            foreach (var file in files)
            {
                if (CompileFile(file, to))
                    successes++;
            }
            return (successes,files.Length);
        }
        public static void WriteSettings()
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                var json = System.Text.Json.JsonSerializer.Serialize(Settings, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (SystemException e)
            {
                WriteLineVerbose($"[Error writing settings file: {e}");
            }
        }
        public static void ReadSettings()
        {
            try
            {
                var json = File.ReadAllText(SettingsPath);
                CompilerSettings? settings;
                if ((settings = System.Text.Json.JsonSerializer.Deserialize<CompilerSettings>(json)) != null)
                    Settings = settings;
            }
            catch (SystemException e)
            {
                WriteLineVerbose($"[Error reading settings file: {e}");
            }
            catch (System.Text.Json.JsonException e)
            {
                WriteLineVerbose($"[Error reading settings file: {e}");
            }
        }
        public static string GetParentAddonName(string folderPath)
        {
            var addonFolder = Steam.SteamData.GetHLAMainAddOnFolder();
            if (!folderPath.Contains(addonFolder) || addonFolder == folderPath)
                return "";
            
            var folders = folderPath.Split(Path.DirectorySeparatorChar);
            for (int i = folders.Length - 1; i >= 0; i--)
            {
                if (folders[i] == "hlvr_addons")
                {
                    return folders[i + 1];
                }
            }

            return "";
            
        }
        private static void WriteLineVerbose(string message)
        {
            if (Settings.Verbose) Console.WriteLine(message);
        }
        private static void WriteVerbose(string message)
        {
            if (Settings.Verbose) Console.Write(message);
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

        private class CompilerSettings
        {
            public bool PauseOnCompletion { get; set; } = false;
            public bool Verbose { get; set; } = false;
        }
    }


    
}
